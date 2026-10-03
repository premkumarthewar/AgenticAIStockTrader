using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Kite.Models;

public sealed record KiteSessionData
{
    [JsonPropertyName("user_id")]
    public string? UserId { get; init; }

    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    [JsonPropertyName("public_token")]
    public string? PublicToken { get; init; }

    [JsonPropertyName("login_time")]
    public string? LoginTime { get; init; }
}
