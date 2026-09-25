namespace StockTrader.Domain.Entities;

/// <summary>
/// A durable record of an alert fired by the background market monitor, so alerts that fire while no client is connected to the live SignalR feed aren't lost.
/// </summary>
public class MonitoringAlert
{
    public Guid Id { get; set; }

    public required string Symbol { get; set; }

    public required string AlertType { get; set; }

    public required string Message { get; set; }

    public DateTime TriggeredAtUtc { get; set; }
}
