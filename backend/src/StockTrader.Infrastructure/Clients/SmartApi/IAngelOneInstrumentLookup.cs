using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.SmartApi;

/// <summary>
/// Angel One's placeOrder needs a numeric symboltoken alongside the plain trading
/// symbol - Kite Connect has no equivalent requirement (see KiteBrokerService).
/// </summary>
public interface IAngelOneInstrumentLookup
{
    Task<Result<string>> GetSymbolTokenAsync(string symbol, string exchange, CancellationToken cancellationToken = default);
}
