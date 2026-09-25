namespace StockTrader.Contracts.Requests;

public sealed class AddWatchlistSymbolRequest
{
    public string Symbol { get; init; } = string.Empty;
}
