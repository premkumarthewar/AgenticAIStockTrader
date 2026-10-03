namespace StockTrader.Infrastructure.Options;

public sealed class AlpacaOptions
{
    public const string SectionName = "Broker:Alpaca";

    /// <summary>
    /// https://paper-api.alpaca.markets for paper trading, https://api.alpaca.markets for
    /// live trading. Defaults to the paper endpoint so a missing/blank config value never
    /// accidentally points at a live account.
    /// </summary>
    public string BaseUrl { get; init; } = "https://paper-api.alpaca.markets";

    public string ApiKeyId { get; init; } = string.Empty;

    public string ApiSecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Purely descriptive - reported back on BrokerAccountDto so callers can tell paper
    /// from live at a glance. Does not itself change BaseUrl; set both consistently.
    /// </summary>
    public bool IsPaperTrading { get; init; } = true;
}
