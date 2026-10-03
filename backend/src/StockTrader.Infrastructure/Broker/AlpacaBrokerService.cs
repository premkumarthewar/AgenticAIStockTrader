using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure.Clients.Alpaca;
using StockTrader.Infrastructure.Clients.Alpaca.Models;
using StockTrader.Infrastructure.Options;
using StockTrader.Shared.Results;
using System.Globalization;

namespace StockTrader.Infrastructure.Broker;

/// <summary>
/// Business-facing broker service (mirrors StockMarketService over IFinnhubClient). Maps
/// the broker-agnostic Broker DTOs onto Alpaca's own request/response shapes so the rest
/// of the app (TradeApprovalService, IBrokerTradeExecutor) never depends on Alpaca types
/// directly.
/// </summary>
public sealed class AlpacaBrokerService(
    IAlpacaClient alpacaClient,
    IOptions<AlpacaOptions> options,
    ILogger<AlpacaBrokerService> logger) : IBrokerClient
{
    private readonly AlpacaOptions _options = options.Value;

    public async Task<Result<BrokerOrderDto>> PlaceOrderAsync(BrokerOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string normalizedSymbol = request.Symbol.Trim().ToUpperInvariant();

        string side = request.Side.Trim().ToUpperInvariant() switch
        {
            "BUY" => "buy",
            "SELL" => "sell",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(side))
            return Result<BrokerOrderDto>.Failure(new Error("BadRequest", $"Unrecognized order side '{request.Side}'."));

        if (request.Quantity <= 0)
            return Result<BrokerOrderDto>.Failure(new Error("BadRequest", "Order quantity must be greater than zero."));

        AlpacaOrderRequest alpacaRequest = new()
        {
            Symbol = normalizedSymbol,
            Quantity = request.Quantity.ToString(CultureInfo.InvariantCulture),
            Side = side,
            Type = request.LimitPrice.HasValue ? "limit" : "market",
            LimitPrice = request.LimitPrice?.ToString(CultureInfo.InvariantCulture),
            ClientOrderId = request.ClientOrderId
        };

        Result<AlpacaOrderResponse> result = await alpacaClient.PlaceOrderAsync(alpacaRequest, cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Broker order for {Symbol} was rejected: {Error}", normalizedSymbol, result.Error.Message);

            return Result<BrokerOrderDto>.Failure(result.Error);
        }

        return Result<BrokerOrderDto>.Success(Map(result.Value));
    }

    public async Task<Result<BrokerOrderDto>> GetOrderAsync(string brokerOrderId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brokerOrderId);

        Result<AlpacaOrderResponse> result = await alpacaClient.GetOrderAsync(brokerOrderId, cancellationToken);

        if (result.IsFailure)
            return Result<BrokerOrderDto>.Failure(result.Error);

        return Result<BrokerOrderDto>.Success(Map(result.Value));
    }

    public async Task<Result<BrokerAccountDto>> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        Result<AlpacaAccountResponse> result = await alpacaClient.GetAccountAsync(cancellationToken);

        if (result.IsFailure)
            return Result<BrokerAccountDto>.Failure(result.Error);

        AlpacaAccountResponse account = result.Value;

        return Result<BrokerAccountDto>.Success(new BrokerAccountDto
        {
            AccountId = account.Id ?? string.Empty,
            CashBalance = ParseDecimal(account.Cash),
            PortfolioValue = ParseDecimal(account.PortfolioValue),
            BuyingPower = ParseDecimal(account.BuyingPower),
            IsPaperTrading = _options.IsPaperTrading,
            TradingBlocked = account.TradingBlocked || account.AccountBlocked
        });
    }

    private static BrokerOrderDto Map(AlpacaOrderResponse order)
    {
        return new BrokerOrderDto
        {
            BrokerOrderId = order.Id ?? string.Empty,
            Symbol = order.Symbol ?? string.Empty,
            Side = order.Side?.ToUpperInvariant() ?? string.Empty,
            Quantity = ParseDecimal(order.Quantity),
            Status = order.Status ?? "unknown",
            FilledQuantity = string.IsNullOrWhiteSpace(order.FilledQuantity) ? null : ParseDecimal(order.FilledQuantity),
            FilledAveragePrice = string.IsNullOrWhiteSpace(order.FilledAveragePrice) ? null : ParseDecimal(order.FilledAveragePrice),
            SubmittedOnUtc = order.SubmittedAt ?? DateTime.UtcNow
        };
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed) ? parsed : 0m;
    }
}
