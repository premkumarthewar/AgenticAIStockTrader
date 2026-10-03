using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.SmartApi.Models;

public sealed record SmartApiRmsData
{
    [JsonPropertyName("net")]
    public string? Net { get; init; }

    [JsonPropertyName("availablecash")]
    public string? AvailableCash { get; init; }

    [JsonPropertyName("availablelimitmargin")]
    public string? AvailableLimitMargin { get; init; }
}
