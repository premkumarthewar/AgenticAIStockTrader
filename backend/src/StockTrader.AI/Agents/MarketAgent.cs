namespace StockTrader.AI.Agents;

public sealed class MarketAgent
{
    public Task<string> AnalyzeAsync(string symbol, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.Empty);
    }
}
