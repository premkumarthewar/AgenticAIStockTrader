using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Kite.Models;

/// <summary>
/// Every Kite Connect v3 endpoint replies with this same {status, data, message,
/// error_type} envelope, so one generic type covers session exchange, order placement,
/// order lookup and margins alike.
/// </summary>
public sealed record KiteApiResponse<T>
{
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("error_type")]
    public string? ErrorType { get; init; }

    public bool IsSuccess => string.Equals(Status, "success", StringComparison.OrdinalIgnoreCase);
}
