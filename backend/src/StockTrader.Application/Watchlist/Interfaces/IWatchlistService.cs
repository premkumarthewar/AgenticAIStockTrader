using StockTrader.Application.Watchlist.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Watchlist.Interfaces;

public interface IWatchlistService
{
    Task<Result<IReadOnlyList<WatchlistItemDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<WatchlistItemDto>> AddAsync(string symbol, CancellationToken cancellationToken = default);

    Task<Result> RemoveAsync(string symbol, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists an alert fired by the background market monitor, so it isn't lost if no
    /// client was connected to the live feed when it fired.
    /// </summary>
    Task RecordAlertAsync(MonitoringAlertDto alert, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<MonitoringAlertDto>>> GetRecentAlertsAsync(int count, CancellationToken cancellationToken = default);
}
