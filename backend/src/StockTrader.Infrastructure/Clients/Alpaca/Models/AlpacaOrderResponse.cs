using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Alpaca.Models;

public sealed record AlpacaOrderResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("client_order_id")]
    public string? ClientOrderId { get; init; }

    [JsonPropertyName("symbol")]
    public string? Symbol { get; init; }

    [JsonPropertyName("side")]
    public string? Side { get; init; }

    [JsonPropertyName("qty")]
    public string? Quantity { get; init; }

    [JsonPropertyName("filled_qty")]
    public string? FilledQuantity { get; init; }

    [JsonPropertyName("filled_avg_price")]
    public string? FilledAveragePrice { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("submitted_at")]
    public DateTime? SubmittedAt { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
