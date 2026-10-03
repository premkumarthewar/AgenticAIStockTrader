using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Approvals.Dtos;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Approvals.Interfaces;

public interface ITradeApprovalService
{
    /// <summary>
    /// Creates a Pending TradeApproval from a BUY/SELL trading decision, targeting the
    /// given broker. The caller (TradingAdvisorService) is expected to have already
    /// filtered out HOLD decisions.
    /// </summary>
    Task<Result<TradeApprovalDto>> RequestApprovalAsync(TradingDecisionDto decision, BrokerProvider brokerProvider, CancellationToken cancellationToken = default);

    Task<Result<TradeApprovalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TradeApprovalDto>>> GetPendingAsync(CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<TradeApprovalDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a pending TradeApproval and immediately attempts to place the order with
    /// the broker. The returned DTO reflects the outcome: Status is Executed with
    /// BrokerOrderId set on success, or ExecutionFailed with ExecutionError set if the
    /// broker call itself failed after approval.
    /// </summary>
    Task<Result<TradeApprovalDto>> ApproveAsync(Guid id, string? notes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a pending TradeApproval. No broker call is ever made for a rejected trade.
    /// </summary>
    Task<Result<TradeApprovalDto>> RejectAsync(Guid id, string? notes, CancellationToken cancellationToken = default);
}
