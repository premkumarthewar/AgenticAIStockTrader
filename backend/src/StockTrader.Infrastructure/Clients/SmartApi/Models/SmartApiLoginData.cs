using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.SmartApi.Models;

public sealed record SmartApiLoginData
{
    [JsonPropertyName("jwtToken")]
    public string? JwtToken { get; init; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; init; }

    [JsonPropertyName("feedToken")]
    public string? FeedToken { get; init; }
}
