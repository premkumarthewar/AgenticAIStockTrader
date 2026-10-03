namespace StockTrader.Domain.Entities;

/// <summary>
/// A BUY/SELL trading decision that is held for human sign-off before it is ever sent to
/// the broker. Created by TradingAdvisorService whenever AnalyzeAndRequestApprovalAsync
/// produces a BUY or SELL decision; a human then approves or rejects it through
/// ApprovalsController, and only an approval triggers a real broker order.
/// </summary>
public sealed class TradeApproval
{
    public Guid Id { get; set; }

    public required string Symbol { get; set; }

    public required string Action { get; set; }

    public decimal Quantity { get; set; }

    public decimal TargetPrice { get; set; }

    public decimal Confidence { get; set; }

    public required string RiskLevel { get; set; }

    public required string Reasoning { get; set; }

    /// <summary>
    /// Which broker this trade will be routed to once approved. Set explicitly when the
    /// approval is opened (TradeApprovalService.RequestApprovalAsync) rather than
    /// inferred from the symbol, so the human reviewing the approval always knows where
    /// it's headed before signing off on a real order.
    /// </summary>
    public BrokerProvider BrokerProvider { get; set; }

    public TradeApprovalStatus Status { get; set; } = TradeApprovalStatus.Pending;

    public DateTime RequestedOnUtc { get; set; }

    public DateTime? DecidedOnUtc { get; set; }

    public string? DecisionNotes { get; set; }

    /// <summary>
    /// Order id returned by the broker once the approved trade has been submitted.
    /// Null until Status becomes Executed.
    /// </summary>
    public string? BrokerOrderId { get; set; }

    /// <summary>
    /// Populated when Status is ExecutionFailed, explaining why the broker rejected or
    /// could not accept the order after a human had already approved it.
    /// </summary>
    public string? ExecutionError { get; set; }
}
