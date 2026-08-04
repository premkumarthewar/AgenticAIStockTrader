using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockTrader.AI.Agents;
using StockTrader.AI.Options;
using StockTrader.AI.Services;
using StockTrader.Application.Common.Interfaces;

namespace StockTrader.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddAI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AIOptions>(configuration.GetSection(AIOptions.SectionName));

        services.AddSingleton<MarketAgent>();

        services.AddScoped<ITradingAdvisorService, TradingAdvisorService>();

        return services;
    }
}
