using StockTrader.Application.Broker.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Common.Interfaces;

/// <summary>
/// Business-facing abstraction over a real brokerage (mirrors IStockMarketService for
/// market data). Implemented in Infrastructure by AlpacaBrokerService, which wraps the
/// low-level IAlpacaClient. Only ever called after a human has approved a trade.
/// </summary>
public interface IBrokerClient
{
    Task<Result<BrokerOrderDto>> PlaceOrderAsync(BrokerOrderRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<BrokerOrderDto>> GetOrderAsync(string brokerOrderId, CancellationToken cancellationToken = default);

    Task<Result<BrokerAccountDto>> GetAccountAsync(CancellationToken cancellationToken = default);
}
