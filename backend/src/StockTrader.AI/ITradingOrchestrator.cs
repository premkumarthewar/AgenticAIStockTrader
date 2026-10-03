using StockTrader.Application.AI.Dtos;
using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.AI;

public interface ITradingOrchestrator
{
    Task<Result<TradingDecisionDto>> AnalyzeAsync(Guid userId, AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken = default);
}
