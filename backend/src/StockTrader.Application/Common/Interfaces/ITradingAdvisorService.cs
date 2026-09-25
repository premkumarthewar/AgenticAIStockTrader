using StockTrader.Application.AI.Dtos;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

public interface ITradingAdvisorService
{
    Task<Result<TradingDecisionDto>> AnalyzeAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the same analysis as AnalyzeAsync, then automatically places the resulting BUY/SELL as a paper trade (a HOLD, or a BUY the risk check overrode to HOLD, places no trade). Unlike AnalyzeAsync, this has a side effect on the paper trading portfolio.
    /// </summary>
    Task<Result<ExecutionResultDto>> AnalyzeAndExecuteAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default);

    Task<Result<AnalyzeStockResponse>> AnalyzeMarketAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default);

    Task<Result<AnalyzeStockResponse>> ResearchAsync(AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken = default);

    Task<Result<PortfolioRecommendationDto>> AnalyzePortfolioAsync(PortfolioAnalysisRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<WatchlistAnalysisDto>> AnalyzeWatchlistAsync(WatchlistDto request, CancellationToken cancellationToken = default);

    Task<Result<MemoryResponseDto>> GetMemoryAsync(string symbol, CancellationToken cancellationToken = default);
}
