namespace StockTrader.Infrastructure.Options
{
    public class FinnhubOptions
    {
        public const string SectionName = "Finnhub";

        public string BaseUrl { get; init; } = string.Empty;

        public string ApiKey { get; init; } = string.Empty;
    }
}
