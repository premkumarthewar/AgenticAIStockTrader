using System.Text.Json.Serialization;

namespace StockTrader.Infrastructure.Clients.Finnhub.Models
{
    public sealed class FinnhubCompanyProfileResponse
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = string.Empty;

        [JsonPropertyName("finnhubIndustry")]
        public string Industry { get; set; } = string.Empty;

        [JsonPropertyName("weburl")]
        public string Website { get; set; } = string.Empty;

        [JsonPropertyName("marketCapitalization")]
        public decimal MarketCapitalization { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
