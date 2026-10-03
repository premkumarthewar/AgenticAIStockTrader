using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Alpaca.Models;

/// <summary>
/// Request body for POST /v2/orders. Alpaca expects quantity and price as strings, not
/// JSON numbers.
/// </summary>
public sealed record AlpacaOrderRequest
{
    [JsonPropertyName("symbol")]
    public required string Symbol { get; init; }

    [JsonPropertyName("qty")]
    public required string Quantity { get; init; }

    [JsonPropertyName("side")]
    public required string Side { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("time_in_force")]
    public string TimeInForce { get; init; } = "day";

    [JsonPropertyName("limit_price")]
    public string? LimitPrice { get; init; }

    [JsonPropertyName("client_order_id")]
    public string? ClientOrderId { get; init; }
}
