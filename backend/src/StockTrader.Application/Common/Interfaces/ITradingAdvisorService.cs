using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Approvals.Dtos;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

public interface ITradingAdvisorService
{
    Task<Result<TradingDecisionDto>> AnalyzeAsync(Guid userId, AnalyzeStockRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the same analysis as AnalyzeAsync, then automatically places the resulting
    /// BUY/SELL as a paper trade (a HOLD, or a BUY the risk check overrode to HOLD,
    /// places no trade). Unlike AnalyzeAsync, this has a side effect on the paper
    /// trading portfolio. This is simulated money only - it never touches the broker
    /// and never requires human approval; see AnalyzeAndRequestApprovalAsync for that.
    /// </summary>
    Task<Result<ExecutionResultDto>> AnalyzeAndExecuteAsync(Guid userId, AnalyzeStockRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs the same analysis as AnalyzeAsync, then, for a BUY/SELL decision, opens a
    /// Pending TradeApproval targeting brokerProvider instead of executing anything - a
    /// human must approve it through ApprovalsController before it is ever sent to the
    /// broker. A HOLD decision (or a BUY the risk check overrode to HOLD) returns a
    /// TradeApprovalDto with Status "NotRequired" and never touches the database.
    /// </summary>
    Task<Result<TradeApprovalDto>> AnalyzeAndRequestApprovalAsync(Guid userId, AnalyzeStockRequest request, BrokerProvider brokerProvider, CancellationToken cancellationToken = default);

    Task<Result<AnalyzeStockResponse>> AnalyzeMarketAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default);

    Task<Result<AnalyzeStockResponse>> ResearchAsync(AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken = default);

    Task<Result<PortfolioRecommendationDto>> AnalyzePortfolioAsync(PortfolioAnalysisRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<WatchlistAnalysisDto>> AnalyzeWatchlistAsync(WatchlistDto request, CancellationToken cancellationToken = default);

    Task<Result<MemoryResponseDto>> GetMemoryAsync(string symbol, CancellationToken cancellationToken = default);
}
