using StockTrader.AI;
using StockTrader.Api.BackgroundServices;
using StockTrader.Api.Configurations;
using StockTrader.Api.Hubs;
using StockTrader.Api.Middleware;
using StockTrader.Application;
using StockTrader.Infrastructure;
using StockTrader.Persistence;

const string CorsPolicyName = "StockTraderCorsPolicy";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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

app.UseAuthorization();

app.MapControllers();

app.MapHub<MarketMonitoringHub>("/hubs/market-monitoring");

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
