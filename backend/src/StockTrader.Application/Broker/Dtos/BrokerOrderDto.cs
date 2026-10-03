namespace StockTrader.Application.Broker.Dtos;

public sealed record BrokerOrderDto
{
    public required string BrokerOrderId { get; init; }

    public required string Symbol { get; init; }

    public required string Side { get; init; }

    public required decimal Quantity { get; init; }

    /// <summary>
    /// The broker's own order status string (e.g. Alpaca's "accepted", "filled",
    /// "pending_new", "rejected"), passed through unchanged rather than remapped.
    /// </summary>
    public required string Status { get; init; }

    public decimal? FilledQuantity { get; init; }

    public decimal? FilledAveragePrice { get; init; }

    public DateTime SubmittedOnUtc { get; init; }
}
