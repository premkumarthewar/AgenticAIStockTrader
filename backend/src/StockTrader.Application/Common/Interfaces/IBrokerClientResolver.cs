using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

/// <summary>
/// Picks the IBrokerClient for a given broker at execution time. Implemented in
/// Infrastructure over .NET's keyed DI services - one IBrokerClient is registered per
/// broker under a key matching that broker's BrokerProvider enum name exactly ("Alpaca",
/// "Zerodha", "AngelOne"), so multiple brokers can be active in the same running
/// instance and a TradeApproval's own BrokerProvider decides which one it reaches.
/// </summary>
public interface IBrokerClientResolver
{
    Result<IBrokerClient> Resolve(string brokerProvider);
}
