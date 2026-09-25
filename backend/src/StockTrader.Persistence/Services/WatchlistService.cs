using Microsoft.EntityFrameworkCore;
using StockTrader.Application.Watchlist.Dtos;
using StockTrader.Application.Watchlist.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Persistence.Context;
using StockTrader.Shared.Results;

namespace StockTrader.Persistence.Services;

public sealed class WatchlistService(StockTraderDbContext dbContext) : IWatchlistService
{
    public async Task<Result<IReadOnlyList<WatchlistItemDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<WatchlistItemDto> items = await dbContext.WatchlistItems
            .OrderBy(x => x.Symbol)
            .Select(x => new WatchlistItemDto
            {
                Symbol = x.Symbol,
                AddedOnUtc = x.AddedOnUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<WatchlistItemDto>>.Success(items);
    }

    public async Task<Result<WatchlistItemDto>> AddAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return Result<WatchlistItemDto>.Failure(new Error("BadRequest", "Stock symbol is required."));

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        bool alreadyWatched = await dbContext.WatchlistItems
            .AnyAsync(x => x.Symbol == normalizedSymbol, cancellationToken);

        if (alreadyWatched)
            return Result<WatchlistItemDto>.Failure(new Error("Conflict", $"{normalizedSymbol} is already on the watchlist."));

        WatchlistItem item = new()
        {
            Id = Guid.NewGuid(),
            Symbol = normalizedSymbol,
            AddedOnUtc = DateTime.UtcNow
        };

        dbContext.WatchlistItems.Add(item);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<WatchlistItemDto>.Success(new WatchlistItemDto
        {
            Symbol = item.Symbol,
            AddedOnUtc = item.AddedOnUtc
        });
    }

    public async Task<Result> RemoveAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return Result.Failure(new Error("BadRequest", "Stock symbol is required."));

        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        WatchlistItem? item = await dbContext.WatchlistItems
            .FirstOrDefaultAsync(x => x.Symbol == normalizedSymbol, cancellationToken);

        if (item is null)
            return Result.Failure(new Error("NotFound", $"{normalizedSymbol} is not on the watchlist."));

        dbContext.WatchlistItems.Remove(item);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task RecordAlertAsync(MonitoringAlertDto alert, CancellationToken cancellationToken = default)
    {
        dbContext.MonitoringAlerts.Add(new MonitoringAlert
        {
            Id = Guid.NewGuid(),
            Symbol = alert.Symbol,
            AlertType = alert.AlertType,
            Message = alert.Message,
            TriggeredAtUtc = alert.TriggeredAtUtc
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Result<IReadOnlyList<MonitoringAlertDto>>> GetRecentAlertsAsync(int count, CancellationToken cancellationToken = default)
    {
        int take = count > 0 ? count : 50;

        List<MonitoringAlertDto> alerts = await dbContext.MonitoringAlerts
            .OrderByDescending(x => x.TriggeredAtUtc)
            .Take(take)
            .Select(x => new MonitoringAlertDto
            {
                Symbol = x.Symbol,
                AlertType = x.AlertType,
                Message = x.Message,
                TriggeredAtUtc = x.TriggeredAtUtc
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MonitoringAlertDto>>.Success(alerts);
    }
}
