namespace StockTrader.Domain.Entities;

/// <summary>
/// Lifecycle of a trade decision waiting on a human before it reaches the broker.
/// Pending -&gt; Rejected ends the workflow with no broker call ever made.
/// Pending -&gt; Approved -&gt; Executed is the happy path: the human approved it and the
/// broker accepted the order. Pending -&gt; Approved -&gt; ExecutionFailed means a human signed
/// off but the broker call itself failed (e.g. account restricted, symbol not tradable);
/// it is not re-attempted automatically.
/// </summary>
public enum TradeApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Executed = 3,
    ExecutionFailed = 4
}
