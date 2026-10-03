namespace StockTrader.Domain.Entities;

/// <summary>
/// The broker a TradeApproval will be routed to once a human approves it. Multiple
/// brokers can be active in the same running instance - BrokerClientResolver in
/// Infrastructure picks the matching IBrokerClient by name at execution time.
/// </summary>
public enum BrokerProvider
{
    Alpaca = 0,
    Zerodha = 1,
    AngelOne = 2
}
