using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

public interface ITradingAdvisorService
{
    Task<Result<AnalyzeStockResponse>> AnalyzeMarketAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default);
}
