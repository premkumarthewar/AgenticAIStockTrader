using StockTrader.AI.Agents.Interfaces;

namespace StockTrader.AI.Agents.Factory;

/// <summary>
/// Default implementation of <see cref="IAgentFactory"/>.
/// </summary>
public sealed class AgentFactory(
    IMarketAgent marketAgent,
    IResearchAgent researchAgent,
    ITradingDecisionAgent tradingDecisionAgent,
    IPortfolioAgent portfolioAgent,
    IRiskAgent riskAgent,
    IExecutionAgent executionAgent,
    ITradingOrchestrator tradingOrchestrator) : IAgentFactory
{
    public IMarketAgent CreateMarketAgent() => marketAgent;
    public IResearchAgent CreateResearchAgent() => researchAgent;
    public ITradingDecisionAgent CreateTradingDecisionAgent() => tradingDecisionAgent;
    public IPortfolioAgent CreatePortfolioAgent() => portfolioAgent;
    public IRiskAgent CreateRiskAgent() => riskAgent;
    public IExecutionAgent CreateExecutionAgent() => executionAgent;
    public ITradingOrchestrator CreateTradingOrchestrator() => tradingOrchestrator;
}