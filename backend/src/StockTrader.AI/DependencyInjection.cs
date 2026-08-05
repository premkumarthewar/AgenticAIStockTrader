using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockTrader.AI.Agents;
using StockTrader.AI.Agents.Factory;
using StockTrader.AI.Kernel;
using StockTrader.AI.Options;
using StockTrader.AI.Services;
using StockTrader.Application.Common.Interfaces;

namespace StockTrader.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddArtificialIntelligence(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<AIOptions>(configuration.GetSection(AIOptions.SectionName));

        services.AddSingleton<IKernelFactory, KernelFactory>();

        services.AddScoped<IAgentFactory, AgentFactory>();

        // services.AddScoped<IMarketAgent, MarketAgent>();
        // services.AddScoped<IResearchAgent, ResearchAgent>();
        // services.AddScoped<IPortfolioAgent, PortfolioAgent>();
        // services.AddScoped<IRiskAgent, RiskAgent>();
        // services.AddScoped<IExecutionAgent, ExecutionAgent>();
        // services.AddScoped<IOrchestratorAgent, OrchestratorAgent>();

        return services;
    }
}
