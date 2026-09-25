namespace StockTrader.Application.Watchlist.Dtos;

/// <summary>
/// One alert fired by the background market monitor - the payload persisted to history
/// and pushed live over the MarketMonitoringHub SignalR hub.
/// </summary>
public sealed record MonitoringAlertDto
{
    public required string Symbol { get; init; }

    public required string AlertType { get; init; }

    public required string Message { get; init; }

    public DateTime TriggeredAtUtc { get; init; }
}
