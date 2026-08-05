using StockTrader.AI.Agents.Interfaces;

namespace StockTrader.AI.Agents.Factory;

/// <summary>
/// Default implementation of <see cref="IAgentFactory"/>.
/// </summary>
public sealed class AgentFactory(
    IMarketAgent marketAgent,
    IResearchAgent researchAgent,
    IPortfolioAgent portfolioAgent,
    IRiskAgent riskAgent,
    IExecutionAgent executionAgent,
    IOrchestratorAgent orchestratorAgent) : IAgentFactory
{
    public IMarketAgent CreateMarketAgent() => marketAgent;
    public IResearchAgent CreateResearchAgent() => researchAgent;
    public IPortfolioAgent CreatePortfolioAgent() => portfolioAgent;
    public IRiskAgent CreateRiskAgent() => riskAgent;
    public IExecutionAgent CreateExecutionAgent() => executionAgent;
    public IOrchestratorAgent CreateOrchestratorAgent() => orchestratorAgent;
}