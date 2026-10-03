using StockTrader.Infrastructure.Clients.Alpaca.Models;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.Alpaca;

public interface IAlpacaClient
{
    Task<Result<AlpacaOrderResponse>> PlaceOrderAsync(AlpacaOrderRequest request, CancellationToken cancellationToken = default);

    Task<Result<AlpacaOrderResponse>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

    Task<Result<AlpacaAccountResponse>> GetAccountAsync(CancellationToken cancellationToken = default);
}
