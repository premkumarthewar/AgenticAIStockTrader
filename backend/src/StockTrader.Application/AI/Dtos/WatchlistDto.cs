namespace StockTrader.Application.AI.Dtos;

public sealed record WatchlistDto
{
    public IReadOnlyList<string> Symbols { get; init; } = [];
}
