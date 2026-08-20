using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        //1. Configuration
        services.Configure<AIOptions>(configuration.GetSection(AIOptions.SectionName));

        //2. Kernel
        //services.AddSingleton<IKernelFactory, KernelFactory>();
        services.AddSingleton(serviceProvider =>
        {
            AIOptions options = serviceProvider.GetRequiredService<IOptions<AIOptions>>().Value;

            return Microsoft.SemanticKernel.Kernel.CreateBuilder().AddOpenAIChatCompletion(modelId: options.Model, apiKey: options.ApiKey).Build();
        });

        //3. Agent Context
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

        //4. Plugins
        services.AddScoped<CompanyProfilePlugin>();
        services.AddScoped<StockQuotePlugin>();
        services.AddScoped<HistoricalPricePlugin>();
        services.AddScoped<FinancialsPlugin>();
        services.AddScoped<NewsPlugin>();

        //5. Agents
        services.AddScoped<IMarketAgent, MarketAgent>();
        services.AddScoped<IResearchAgent, ResearchAgent>();
        services.AddScoped<ITradingDecisionAgent, TradingDecisionAgent>();
        services.AddScoped<IPortfolioAgent, PortfolioAgent>();
        services.AddScoped<IRiskAgent, RiskAgent>();
        services.AddScoped<IExecutionAgent, ExecutionAgent>();

        //6. Agent Factory
        services.AddScoped<IAgentFactory, AgentFactory>();

        //7. Orchestrator
        services.AddScoped<ITradingOrchestrator, TradingOrchestrator>();

        //8. Application Services
        services.AddScoped<ITradingAdvisorService, TradingAdvisorService>();

        return services;
    }
}
