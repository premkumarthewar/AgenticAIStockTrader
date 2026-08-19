using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces;

public interface IResearchAgent
{
    Task<Result<string>> ResearchAsync(string symbol, CancellationToken cancellationToken = default);
}
