using StockTrader.AI;
using StockTrader.AI.Agents;
using StockTrader.AI.Options;
using StockTrader.AI.Plugins.CompanyProfile;
using StockTrader.AI.Services;
using StockTrader.Application;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure;
using StockTrader.Infrastructure.Clients.Finnhub;
using StockTrader.Infrastructure.MarketData;
using StockTrader.Infrastructure.Options;
using StockTrader.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddArtificialIntelligence(builder.Configuration);

builder.Services.Configure<AIOptions>(builder.Configuration.GetSection(AIOptions.SectionName));

builder.Services.AddSingleton<MarketAgent>();

builder.Services.AddScoped<ITradingAdvisorService, TradingAdvisorService>();

builder.Services.Configure<FinnhubOptions>(builder.Configuration.GetSection(FinnhubOptions.SectionName));

builder.Services.AddHttpClient<IFinnhubClient, FinnhubClient>();

builder.Services.AddScoped<IStockMarketService, StockMarketService>();

builder.Services.AddScoped<CompanyProfilePlugin>();

builder.Services.AddSingleton<MarketAgent>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
