using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using StockTrader.AI;
using StockTrader.Api.BackgroundServices;
using StockTrader.Api.Configurations;
using StockTrader.Api.Extensions;
using StockTrader.Api.HealthChecks;
using StockTrader.Api.Hubs;
using StockTrader.Api.Middleware;
using StockTrader.Api.Services;
using StockTrader.Application;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure;
using StockTrader.Persistence;

const string CorsPolicyName = "StockTraderCorsPolicy";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Structured logging: Serilog replaces the default Microsoft.Extensions.Logging console
// provider entirely, reading its sinks/levels from the "Serilog" section of
// appsettings.json (ReadFrom.Configuration) so behavior differs by environment without a
// code change. ReadFrom.Services lets sinks/enrichers registered in DI (none today) be
// picked up automatically; Enrich.FromLogContext lets ad-hoc LogContext.PushProperty
// scopes flow into every log event within them.
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Global exception handling: any unhandled exception is converted into a
// consistent ProblemDetails JSON response by GlobalExceptionHandler instead
// of crashing the request or leaking a raw stack trace to the client.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS: allowed origins come from configuration (Cors:AllowedOrigins in
// appsettings.json / appsettings.{Environment}.json) so the frontend's origin
// can be set per-environment without a code change. No origins configured
// means no cross-origin requests are allowed.
string[] allowedOrigins =
    builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// Authentication/authorization: JWT bearer tokens issued by AuthService (Persistence),
// validated here against the same "Jwt" configuration section. See
// Extensions/JwtAuthenticationExtensions.cs. AddJwtAuthentication also registers
// AddAuthorization, so every [Authorize] attribute in the API is backed by this scheme.
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Health checks: "live" answers "is the process up" (no external dependency, always
// fast); "ready" additionally verifies the database is reachable, since a pod that's
// running but can't reach SQL Server shouldn't be sent traffic by an orchestrator.
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing required 'ConnectionStrings:DefaultConnection' configuration.");

builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sql-server", tags: ["ready"])
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

// Distributed tracing/metrics via OpenTelemetry. With no "Telemetry:OtlpEndpoint"
// configured (the default in every environment today) spans/metrics go to the console
// exporter, which is enough to see that instrumentation is wired correctly without
// standing up a collector; setting that config value switches to OTLP export instead.
TelemetryOptions telemetryOptions = builder.Configuration.GetSection(TelemetryOptions.SectionName).Get<TelemetryOptions>()
    ?? new TelemetryOptions();

bool useOtlpExporter = !string.IsNullOrWhiteSpace(telemetryOptions.OtlpEndpoint);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: telemetryOptions.ServiceName))
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();

        if (useOtlpExporter)
            tracing.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(telemetryOptions.OtlpEndpoint!));
        else
            tracing.AddConsoleExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation();
        metrics.AddHttpClientInstrumentation();
        metrics.AddRuntimeInstrumentation();

        if (useOtlpExporter)
            metrics.AddOtlpExporter(otlp => otlp.Endpoint = new Uri(telemetryOptions.OtlpEndpoint!));
        else
            metrics.AddConsoleExporter();
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddArtificialIntelligence(builder.Configuration);

builder.Services.AddPersistence(builder.Configuration);

// Live market monitoring: a background service polls the persisted watchlist and
// pushes any price alerts to connected clients over SignalR.
builder.Services.Configure<MarketMonitoringOptions>(
    builder.Configuration.GetSection(MarketMonitoringOptions.SectionName));

builder.Services.AddSignalR();

builder.Services.AddHostedService<MarketMonitoringBackgroundService>();

WebApplication app = builder.Build();

// Registered first so it can catch exceptions thrown by any middleware below it.
app.UseExceptionHandler();

// Every request logged as one structured event (method, path, status code, elapsed time,
// plus anything already in the Serilog LogContext) rather than the several
// framework-internal log lines ASP.NET Core would otherwise emit per request.
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    // So that hitting the app's base URL (e.g. http://localhost:5032) lands
    // somewhere useful instead of a 404, since no other endpoint is mapped to "/".
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicyName);

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MarketMonitoringHub>("/hubs/market-monitoring");

// Health endpoints are deliberately anonymous - an orchestrator's liveness/readiness
// probe carries no bearer token - and split by tag so a slow/unreachable database only
// ever fails readiness, never liveness (which would otherwise cause an unnecessary
// container restart for a problem a restart can't fix).
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync
}).AllowAnonymous();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Exposed so WebApplicationFactory<Program> (used by integration tests) can discover
// this entry point - top-level statements generate an internal Program class by default,
// which a test project in a different assembly can't reference otherwise.
[ExcludeFromCodeCoverage]
public partial class Program;
