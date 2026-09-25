using StockTrader.AI.Agents.Interfaces;

namespace StockTrader.AI.Agents.Factory;

/// <summary>
/// Creates fully configured AI agents.
/// </summary>
public interface IAgentFactory
{
    IMarketAgent CreateMarketAgent();

    IResearchAgent CreateResearchAgent();

    ITradingDecisionAgent CreateTradingDecisionAgent();

    IPortfolioAgent CreatePortfolioAgent();

    IExecutionAgent CreateExecutionAgent();

    ITradingOrchestrator CreateTradingOrchestrator();

    IWatchlistAgent CreateWatchlistAgent();
}
