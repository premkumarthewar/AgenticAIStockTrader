using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.SmartApi.Models;

/// <summary>
/// Every SmartAPI endpoint replies with this same {status, message, errorcode, data}
/// envelope.
/// </summary>
public sealed record SmartApiResponse<T>
{
    [JsonPropertyName("status")]
    public bool Status { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("errorcode")]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("data")]
    public T? Data { get; init; }
}
