using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces;

public interface IResearchAgent
{
    Task<Result<string>> ResearchAsync(AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken = default);
}
