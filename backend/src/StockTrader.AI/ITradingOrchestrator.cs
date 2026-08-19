using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.AI;

public interface ITradingOrchestrator
{
    Task<Result<AnalyzeStockResponse>> AnalyzeAsync(AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken = default);
}
