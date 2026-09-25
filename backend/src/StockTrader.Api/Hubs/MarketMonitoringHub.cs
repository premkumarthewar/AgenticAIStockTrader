using Microsoft.AspNetCore.SignalR;

namespace StockTrader.Api.Hubs;

/// <summary>
/// Live push channel for background market-monitoring alerts. MarketMonitoringBackgroundService
/// broadcasts a "ReceiveAlert" message (payload: MonitoringAlertDto) to every connected
/// client whenever a watched symbol's price moves past the alert threshold. Clients
/// don't need to call anything on the hub - it's a pure server-to-client push channel.
/// </summary>
public sealed class MarketMonitoringHub : Hub
{
}
