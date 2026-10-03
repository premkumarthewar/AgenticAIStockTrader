using StockTrader.Infrastructure.Clients.SmartApi.Models;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Clients.SmartApi;

public interface ISmartApiClient
{
    Task<Result<SmartApiOrderData>> PlaceOrderAsync(
        string tradingSymbol,
        string symbolToken,
        string exchange,
        string transactionType,
        string orderType,
        string productType,
        decimal quantity,
        decimal? price,
        CancellationToken cancellationToken = default);

    Task<Result<SmartApiOrderBookEntry>> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

    Task<Result<SmartApiRmsData>> GetRmsAsync(CancellationToken cancellationToken = default);
}
