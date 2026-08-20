using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure.Clients.Finnhub;
using StockTrader.Infrastructure.MarketData;
using StockTrader.Infrastructure.Options;

namespace StockTrader.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        // Market API
        // Broker API
        // Email
        // Logging

        services.Configure<FinnhubOptions>(configuration.GetSection("FinnHub"));

        // Lowest level. External API client.
        services.AddHttpClient<IFinnhubClient, FinnhubClient>();

        // Business service. Application-facing service.
        services.AddScoped<IStockMarketService, StockMarketService>();

        return services;
    }
}
