using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using StockTrader.AI.Agents;
using StockTrader.AI.Agents.Base;
using StockTrader.AI.Agents.Factory;
using StockTrader.AI.Agents.Interfaces;
using StockTrader.AI.Kernel;
using StockTrader.AI.Options;
using StockTrader.AI.Plugins.CompanyProfile;
using StockTrader.AI.Plugins.Financials;
using StockTrader.AI.Plugins.HistoricalPrice;
using StockTrader.AI.Plugins.News;
using StockTrader.AI.Plugins.Quotes;
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

        services.AddScoped<AgentContext>(serviceProvider =>
        {
            IKernelFactory kernelFactory = serviceProvider.GetRequiredService<IKernelFactory>();

            ILogger<MarketAgent> logger = serviceProvider.GetRequiredService<ILogger<MarketAgent>>();

            Microsoft.SemanticKernel.Kernel kernel = kernelFactory.CreateKernel();

            OpenAIPromptExecutionSettings settings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            return new(kernel, settings, logger);
        });

        services.AddScoped<CompanyProfilePlugin>();
        services.AddScoped<StockQuotePlugin>();
        services.AddScoped<HistoricalPricePlugin>();

        services.AddScoped<FinancialsPlugin>();
        services.AddScoped<NewsPlugin>();

        services.AddScoped<IMarketAgent, MarketAgent>();
        services.AddScoped<IResearchAgent, ResearchAgent>();

        services.AddScoped<ITradingDecisionAgent, TradingDecisionAgent>();

        services.AddScoped<IAgentFactory, AgentFactory>();

        services.AddScoped<ITradingOrchestrator, TradingOrchestrator>();

        services.AddScoped<ITradingAdvisorService, TradingAdvisorService>();

        // services.AddScoped<IResearchAgent, ResearchAgent>();
        // services.AddScoped<IPortfolioAgent, PortfolioAgent>();
        // services.AddScoped<IRiskAgent, RiskAgent>();
        // services.AddScoped<IExecutionAgent, ExecutionAgent>();


        return services;
    }
}
