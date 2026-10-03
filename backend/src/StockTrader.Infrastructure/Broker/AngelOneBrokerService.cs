using Microsoft.Extensions.Logging;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure.Clients.SmartApi;
using StockTrader.Infrastructure.Clients.SmartApi.Models;
using StockTrader.Shared.Results;
using System.Globalization;

namespace StockTrader.Infrastructure.Broker;

/// <summary>
/// Business-facing broker service for Angel One, registered under the keyed "AngelOne"
/// IBrokerClient (see Infrastructure/DependencyInjection.cs). Like Zerodha, SmartAPI has
/// no separate paper-trading endpoint - every order placed here is a live order once
/// AngelOneOptions has real credentials.
/// </summary>
public sealed class AngelOneBrokerService(
    ISmartApiClient smartApiClient,
    IAngelOneInstrumentLookup instrumentLookup,
    ILogger<AngelOneBrokerService> logger) : IBrokerClient
{
    public async Task<Result<BrokerOrderDto>> PlaceOrderAsync(BrokerOrderRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string normalizedSymbol = request.Symbol.Trim().ToUpperInvariant();

        string transactionType = request.Side.Trim().ToUpperInvariant() switch
        {
            "BUY" => "BUY",
            "SELL" => "SELL",
            _ => string.Empty
        };

        if (string.IsNullOrEmpty(transactionType))
            return Result<BrokerOrderDto>.Failure(new Error("BadRequest", $"Unrecognized order side '{request.Side}'."));

        if (request.Quantity <= 0)
            return Result<BrokerOrderDto>.Failure(new Error("BadRequest", "Order quantity must be greater than zero."));

        Result<string> tokenResult = await instrumentLookup.GetSymbolTokenAsync(normalizedSymbol, request.Exchange, cancellationToken);

        if (tokenResult.IsFailure)
            return Result<BrokerOrderDto>.Failure(tokenResult.Error);

        string tradingSymbol = $"{normalizedSymbol}-EQ";

        string orderType = request.LimitPrice.HasValue ? "LIMIT" : "MARKET";

        // DELIVERY (Angel One's equivalent of Zerodha's CNC) is the safer default -
        // INTRADAY/MARGIN are opt-in only via an explicit ProductType.
        string productType = string.IsNullOrWhiteSpace(request.ProductType) ? "DELIVERY" : request.ProductType.Trim().ToUpperInvariant();

        Result<SmartApiOrderData> placeResult = await smartApiClient.PlaceOrderAsync(
            tradingSymbol,
            tokenResult.Value,
            request.Exchange,
            transactionType,
            orderType,
            productType,
            request.Quantity,
            request.LimitPrice,
            cancellationToken);

        if (placeResult.IsFailure)
        {
            logger.LogWarning("Angel One order for {Symbol} was rejected: {Error}", normalizedSymbol, placeResult.Error.Message);

            return Result<BrokerOrderDto>.Failure(placeResult.Error);
        }

        string? orderId = placeResult.Value.OrderId;

        if (string.IsNullOrWhiteSpace(orderId))
            return Result<BrokerOrderDto>.Failure(new Error("BrokerError", "Angel One accepted the order but returned no order id."));

        // Like Kite, placeOrder's own response carries no status - look it up
        // immediately so the caller sees where the order actually landed.
        Result<BrokerOrderDto> statusResult = await GetOrderAsync(orderId, cancellationToken);

        if (statusResult.IsSuccess)
            return statusResult;

        return Result<BrokerOrderDto>.Success(new BrokerOrderDto
        {
            BrokerOrderId = orderId,
            Symbol = normalizedSymbol,
            Side = transactionType,
            Quantity = request.Quantity,
            Status = "submitted",
            SubmittedOnUtc = DateTime.UtcNow
        });
    }

    public async Task<Result<BrokerOrderDto>> GetOrderAsync(string brokerOrderId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(brokerOrderId);

        Result<SmartApiOrderBookEntry> result = await smartApiClient.GetOrderAsync(brokerOrderId, cancellationToken);

        if (result.IsFailure)
            return Result<BrokerOrderDto>.Failure(result.Error);

        SmartApiOrderBookEntry order = result.Value;

        return Result<BrokerOrderDto>.Success(new BrokerOrderDto
        {
            BrokerOrderId = order.OrderId ?? brokerOrderId,
            Symbol = order.TradingSymbol ?? string.Empty,
            Side = order.TransactionType ?? string.Empty,
            Quantity = ParseDecimal(order.Quantity),
            Status = order.Status ?? "unknown",
            FilledQuantity = ParseDecimal(order.FilledShares),
            FilledAveragePrice = ParseDecimal(order.AveragePrice),
            SubmittedOnUtc = ParseUpdateTime(order.UpdateTime)
        });
    }

    public async Task<Result<BrokerAccountDto>> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        Result<SmartApiRmsData> result = await smartApiClient.GetRmsAsync(cancellationToken);

        if (result.IsFailure)
            return Result<BrokerAccountDto>.Failure(result.Error);

        SmartApiRmsData rms = result.Value;

        return Result<BrokerAccountDto>.Success(new BrokerAccountDto
        {
            AccountId = string.Empty,
            CashBalance = ParseDecimal(rms.AvailableCash),
            PortfolioValue = ParseDecimal(rms.Net),
            BuyingPower = ParseDecimal(rms.AvailableLimitMargin),
            IsPaperTrading = false,
            TradingBlocked = false
        });
    }

    private static decimal ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsed) ? parsed : 0m;
    }

    private static DateTime ParseUpdateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.UtcNow;

        // Angel One's order book reports timestamps like "22-Sep-2026 15:04:05" with no
        // timezone marker - treated as IST since that's the only market it serves, then
        // converted to UTC to match every other broker's SubmittedOnUtc.
        if (DateTime.TryParseExact(value, "dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            return parsed.AddHours(-5).AddMinutes(-30);

        return DateTime.UtcNow;
    }
}
