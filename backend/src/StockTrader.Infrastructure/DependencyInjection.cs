using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure.Broker;
using StockTrader.Infrastructure.Clients.Alpaca;
using StockTrader.Infrastructure.Clients.Finnhub;
using StockTrader.Infrastructure.Clients.Kite;
using StockTrader.Infrastructure.Clients.SmartApi;
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

        // Broker APIs. Every one of these is only ever reached through
        // IBrokerTradeExecutor, after a human has approved the trade in
        // ApprovalsController. Multiple brokers are registered side by side as KEYED
        // IBrokerClient services - the key is exactly the BrokerProvider enum name
        // ("Alpaca"/"Zerodha"/"AngelOne"), and BrokerClientResolver looks a TradeApproval's
        // own BrokerProvider up against that key at execution time.

        // --- Alpaca (US equities; paper endpoint by default) ---
        services.Configure<AlpacaOptions>(configuration.GetSection(AlpacaOptions.SectionName));

        services.AddHttpClient<IAlpacaClient, AlpacaClient>();

        services.AddKeyedScoped<IBrokerClient, AlpacaBrokerService>("Alpaca");

        // --- Zerodha / Kite Connect (NSE/BSE; no paper endpoint - always live once
        // credentials are set) ---
        services.Configure<ZerodhaOptions>(configuration.GetSection(ZerodhaOptions.SectionName));

        // KiteSessionProvider caches the day's access_token in memory, so it must be a
        // singleton - AddHttpClient<TClient,TImpl>() registers TClient as transient,
        // which would silently throw the cache away on every resolution. A named client
        // from IHttpClientFactory, handed to a manually-registered singleton, avoids that
        // while still getting pooled-handler reuse from the factory.
        services.AddHttpClient("Kite");

        services.AddSingleton<IKiteSessionProvider>(serviceProvider => new KiteSessionProvider(
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("Kite"),
            serviceProvider.GetRequiredService<IOptions<ZerodhaOptions>>(),
            serviceProvider.GetRequiredService<ILogger<KiteSessionProvider>>()));

        services.AddHttpClient<IKiteConnectClient, KiteConnectClient>();

        services.AddKeyedScoped<IBrokerClient, KiteBrokerService>("Zerodha");

        // --- Angel One / SmartAPI (NSE/BSE; no paper endpoint - always live once
        // credentials are set) ---
        services.Configure<AngelOneOptions>(configuration.GetSection(AngelOneOptions.SectionName));

        // Same reasoning as Kite: SmartApiSessionProvider and AngelOneInstrumentLookup
        // both cache mutable state in memory and must be singletons.
        services.AddHttpClient("AngelOne");

        services.AddSingleton<ISmartApiSessionProvider>(serviceProvider => new SmartApiSessionProvider(
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("AngelOne"),
            serviceProvider.GetRequiredService<IOptions<AngelOneOptions>>(),
            serviceProvider.GetRequiredService<ILogger<SmartApiSessionProvider>>()));

        services.AddHttpClient<ISmartApiClient, SmartApiClient>();

        services.AddHttpClient("AngelOneScripMaster");

        services.AddSingleton<IAngelOneInstrumentLookup>(serviceProvider => new AngelOneInstrumentLookup(
            serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("AngelOneScripMaster"),
            serviceProvider.GetRequiredService<IOptions<AngelOneOptions>>(),
            serviceProvider.GetRequiredService<ILogger<AngelOneInstrumentLookup>>()));

        services.AddKeyedScoped<IBrokerClient, AngelOneBrokerService>("AngelOne");

        // Resolves a TradeApproval's BrokerProvider to one of the keyed IBrokerClient
        // registrations above.
        services.AddScoped<IBrokerClientResolver, BrokerClientResolver>();

        return services;
    }
}
