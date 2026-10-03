using Microsoft.Extensions.Logging;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Infrastructure.Clients.Kite;
using StockTrader.Infrastructure.Clients.Kite.Models;
using StockTrader.Shared.Results;

namespace StockTrader.Infrastructure.Broker;

/// <summary>
/// Business-facing broker service for Zerodha, registered under the keyed "Zerodha"
/// IBrokerClient (see Infrastructure/DependencyInjection.cs). Unlike Alpaca, Kite
/// Connect has no separate paper-trading base URL - once ZerodhaOptions has real
/// credentials and a session is established, every order placed here is a live order.
/// </summary>
public sealed class KiteBrokerService(
    IKiteConnectClient kiteClient,
    ILogger<KiteBrokerService> logger) : IBrokerClient
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

        string orderType = request.LimitPrice.HasValue ? "LIMIT" : "MARKET";

        // CNC (delivery) is the safer default for an equity trading agent - MIS/NRML
        // (intraday/leveraged) are opt-in only via an explicit ProductType.
        string product = string.IsNullOrWhiteSpace(request.ProductType) ? "CNC" : request.ProductType.Trim().ToUpperInvariant();

        Result<KiteOrderData> placeResult = await kiteClient.PlaceOrderAsync(
            normalizedSymbol,
            request.Exchange,
            transactionType,
            orderType,
            request.Quantity,
            product,
            request.LimitPrice,
            cancellationToken);

        if (placeResult.IsFailure)
        {
            logger.LogWarning("Zerodha order for {Symbol} was rejected: {Error}", normalizedSymbol, placeResult.Error.Message);

            return Result<BrokerOrderDto>.Failure(placeResult.Error);
        }

        string? orderId = placeResult.Value.OrderId;

        if (string.IsNullOrWhiteSpace(orderId))
            return Result<BrokerOrderDto>.Failure(new Error("BrokerError", "Zerodha accepted the order but returned no order id."));

        // Kite's place-order response is just the order id - fetch the current status
        // immediately so the caller (and the TradeApproval record) reflects reality
        // rather than a guess.
        Result<BrokerOrderDto> statusResult = await GetOrderAsync(orderId, cancellationToken);

        if (statusResult.IsSuccess)
            return statusResult;

        // The order was accepted even though the follow-up status lookup failed - report
        // it as submitted rather than surfacing the lookup failure as an order failure.
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

        Result<KiteOrderStatus> result = await kiteClient.GetOrderAsync(brokerOrderId, cancellationToken);

        if (result.IsFailure)
            return Result<BrokerOrderDto>.Failure(result.Error);

        KiteOrderStatus status = result.Value;

        return Result<BrokerOrderDto>.Success(new BrokerOrderDto
        {
            BrokerOrderId = status.OrderId ?? brokerOrderId,
            Symbol = status.TradingSymbol ?? string.Empty,
            Side = status.TransactionType ?? string.Empty,
            Quantity = status.Quantity,
            Status = status.Status ?? "unknown",
            FilledQuantity = status.FilledQuantity,
            FilledAveragePrice = status.AveragePrice,
            SubmittedOnUtc = status.OrderTimestamp ?? DateTime.UtcNow
        });
    }

    public async Task<Result<BrokerAccountDto>> GetAccountAsync(CancellationToken cancellationToken = default)
    {
        Result<KiteMarginsData> result = await kiteClient.GetMarginsAsync(cancellationToken);

        if (result.IsFailure)
            return Result<BrokerAccountDto>.Failure(result.Error);

        KiteMarginSegment? equity = result.Value.Equity;

        return Result<BrokerAccountDto>.Success(new BrokerAccountDto
        {
            // Kite's margins endpoint doesn't return an account id - the login response
            // does (KiteSessionData.UserId), but that's not available from here.
            AccountId = string.Empty,
            CashBalance = equity?.Available?.Cash ?? 0m,
            // Zerodha's margins API reports cash/margin balances for the equity segment,
            // not a mark-to-market valuation of current holdings - Net is the closest
            // available figure, not a true total portfolio value.
            PortfolioValue = equity?.Net ?? 0m,
            BuyingPower = equity?.Available?.LiveBalance ?? 0m,
            IsPaperTrading = false,
            TradingBlocked = false
        });
    }
}
