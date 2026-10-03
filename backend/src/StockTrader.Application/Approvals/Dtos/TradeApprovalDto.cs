namespace StockTrader.Application.Approvals.Dtos;

public sealed record TradeApprovalDto
{
    public required Guid Id { get; init; }

    public required string Symbol { get; init; }

    public required string Action { get; init; }

    public decimal Quantity { get; init; }

    public decimal TargetPrice { get; init; }

    public decimal Confidence { get; init; }

    public required string RiskLevel { get; init; }

    public required string Reasoning { get; init; }

    /// <summary>
    /// The enum name of the target BrokerProvider ("Alpaca", "Zerodha", "AngelOne") -
    /// also the exact key BrokerClientResolver.Resolve expects, same convention as the
    /// keyed IBrokerClient registrations in Infrastructure.
    /// </summary>
    public required string BrokerProvider { get; init; }

    public required string Status { get; init; }

    public required DateTime RequestedOnUtc { get; init; }

    public DateTime? DecidedOnUtc { get; init; }

    public string? DecisionNotes { get; init; }

    public string? BrokerOrderId { get; init; }

    public string? ExecutionError { get; init; }
}
