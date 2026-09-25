namespace StockTrader.Application.Watchlist.Dtos;

public sealed record WatchlistItemDto
{
    public required string Symbol { get; init; }

    public DateTime AddedOnUtc { get; init; }
}
