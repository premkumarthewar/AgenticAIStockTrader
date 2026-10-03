using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Kite.Models;

/// <summary>
/// POST /orders/:variety returns just an order_id - the order isn't confirmed filled at
/// that point, only accepted. Use KiteOrderStatus (from GET /orders/:order_id) to see
/// where it actually landed.
/// </summary>
public sealed record KiteOrderData
{
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }
}

/// <summary>
/// One entry in the order's status history, as returned by GET /orders/:order_id (Kite
/// returns every state transition for the order; the last entry in the array is current).
/// </summary>
public sealed record KiteOrderStatus
{
    [JsonPropertyName("order_id")]
    public string? OrderId { get; init; }

    [JsonPropertyName("tradingsymbol")]
    public string? TradingSymbol { get; init; }

    [JsonPropertyName("transaction_type")]
    public string? TransactionType { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; init; }

    [JsonPropertyName("filled_quantity")]
    public decimal FilledQuantity { get; init; }

    [JsonPropertyName("average_price")]
    public decimal AveragePrice { get; init; }

    [JsonPropertyName("order_timestamp")]
    public DateTime? OrderTimestamp { get; init; }
}
