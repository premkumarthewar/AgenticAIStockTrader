using StockTrader.Infrastructure.Clients.Kite.Models;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.Kite;

public interface IKiteConnectClient
{
    Task<Result<KiteOrderData>> PlaceOrderAsync(
        string tradingSymbol,
        string exchange,
        string transactionType,
        string orderType,
        decimal quantity,
        string product,
        decimal? price,
        CancellationToken cancellationToken = default);

    Task<Result<KiteOrderStatus>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

    Task<Result<KiteMarginsData>> GetMarginsAsync(CancellationToken cancellationToken = default);
}
