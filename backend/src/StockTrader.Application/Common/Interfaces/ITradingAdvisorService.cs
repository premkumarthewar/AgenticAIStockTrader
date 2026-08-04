using StockTrader.Contracts.Responses;

namespace StockTrader.Application.Common.Interfaces
{
    public interface ITradingAdvisorService
    {
        Task<AnalyzeStockResponse> AnalyzeStockAsync(string symbol, CancellationToken cancellationToken = default);
    }
}
