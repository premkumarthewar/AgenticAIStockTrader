namespace StockTrader.Application.Broker.Dtos;

/// <summary>
/// A broker-agnostic order request. Side is "BUY" or "SELL" (mapped to the broker's own
/// vocabulary, e.g. Alpaca's "buy"/"sell", inside the client implementation).
/// </summary>
public sealed record BrokerOrderRequestDto
{
    public required string Symbol { get; init; }

    public required string Side { get; init; }

    public required decimal Quantity { get; init; }

    /// <summary>
    /// "market" or "limit". Market orders are used when LimitPrice is null.
    /// </summary>
    public string OrderType { get; init; } = "market";

    public decimal? LimitPrice { get; init; }

    /// <summary>
    /// Caller-supplied idempotency key so a retried approval never places the same order
    /// twice at the broker.
    /// </summary>
    public string? ClientOrderId { get; init; }

    /// <summary>
    /// Exchange to route the order to (e.g. "NSE", "BSE"). Alpaca ignores this - US
    /// equities have no exchange-segment concept in its order API. Defaults to "NSE"
    /// since that's the only exchange the Zerodha/Angel One integrations target today.
    /// </summary>
    public string Exchange { get; init; } = "NSE";

    /// <summary>
    /// Broker-specific product/order type: "CNC"/"MIS"/"NRML" for Zerodha,
    /// "DELIVERY"/"INTRADAY"/"MARGIN" for Angel One. Null lets each broker service apply
    /// its own default (delivery/CNC-equivalent). Alpaca ignores this entirely.
    /// </summary>
    public string? ProductType { get; init; }
}
