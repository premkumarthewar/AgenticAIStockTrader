namespace StockTrader.Infrastructure.Options;

public sealed class ZerodhaOptions
{
    public const string SectionName = "Broker:Zerodha";

    public string ApiBaseUrl { get; init; } = "https://api.kite.trade";

    public string LoginBaseUrl { get; init; } = "https://kite.zerodha.com/connect/login";

    public string ApiKey { get; init; } = string.Empty;

    public string ApiSecret { get; init; } = string.Empty;
}
