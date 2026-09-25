using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using StockTrader.Api.Configurations;
using StockTrader.Api.Hubs;
using StockTrader.Application.Common;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Application.Watchlist.Dtos;
using StockTrader.Application.Watchlist.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.Api.BackgroundServices;

/// <summary>
/// Continuously monitors every symbol on the persisted watchlist and, when a symbol's price has moved past the alert threshold since the last close, records the alert and pushes it live to connected clients over MarketMonitoringHub. Runs for the lifetime of the app on a fixed interval (MarketMonitoring:PollIntervalSeconds).
/// </summary>
public sealed class MarketMonitoringBackgroundService(
    IServiceScopeFactory scopeFactory,
    IHubContext<MarketMonitoringHub> hubContext,
    IOptions<MarketMonitoringOptions> options,
    ILogger<MarketMonitoringBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int pollIntervalSeconds = options.Value.PollIntervalSeconds > 0
            ? options.Value.PollIntervalSeconds
            : 60;

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(pollIntervalSeconds));

        logger.LogInformation(
            "Market monitoring started; checking the watchlist every {IntervalSeconds}s.",
            pollIntervalSeconds);

        try
        {
            do
            {
                try
                {
                    await CheckWatchlistAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // A single failed monitoring cycle (e.g. a transient market-data
                    // outage) must not stop the background service from trying again
                    // on the next tick.
                    logger.LogError(ex, "Market monitoring cycle failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Expected during app shutdown.
        }
    }

    private async Task CheckWatchlistAsync(CancellationToken cancellationToken)
    {
        // BackgroundService lives for the lifetime of the app, so each cycle gets its
        // own DI scope rather than holding scoped services (like the DbContext) open
        // indefinitely.
        using IServiceScope scope = scopeFactory.CreateScope();

        IWatchlistService watchlistService = scope.ServiceProvider.GetRequiredService<IWatchlistService>();

        IStockMarketService stockMarketService = scope.ServiceProvider.GetRequiredService<IStockMarketService>();

        Result<IReadOnlyList<WatchlistItemDto>> watchlistResult = await watchlistService.GetAllAsync(cancellationToken);

        if (watchlistResult.IsFailure || watchlistResult.Value.Count == 0)
            return;

        foreach (WatchlistItemDto item in watchlistResult.Value)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<StockQuoteDto> quoteResult = await stockMarketService.GetQuoteAsync(item.Symbol, cancellationToken);

            if (quoteResult.IsFailure)
            {
                logger.LogWarning(
                    "Market monitoring could not retrieve a quote for {Symbol}: {Error}",
                    item.Symbol,
                    quoteResult.Error.Message);

                continue;
            }

            decimal changePercent = quoteResult.Value.PercentChange;

            string? alertType = PriceAlertEvaluator.Evaluate(changePercent);

            if (alertType is null)
                continue;

            MonitoringAlertDto alert = new()
            {
                Symbol = item.Symbol,
                AlertType = alertType,
                Message = PriceAlertEvaluator.BuildMessage(alertType, changePercent),
                TriggeredAtUtc = DateTime.UtcNow
            };

            await watchlistService.RecordAlertAsync(alert, cancellationToken);

            await hubContext.Clients.All.SendAsync("ReceiveAlert", alert, cancellationToken);

            logger.LogInformation(
                "Market monitoring alert: {Symbol} {AlertType} ({ChangePercent:F2}%)",
                item.Symbol,
                alertType,
                changePercent);
        }
    }
}
