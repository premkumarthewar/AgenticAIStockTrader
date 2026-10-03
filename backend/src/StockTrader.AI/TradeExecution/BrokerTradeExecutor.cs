using StockTrader.AI.TradeExecution.Interfaces;
using StockTrader.Application.Approvals.Dtos;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.AI.TradeExecution;

public sealed class BrokerTradeExecutor(IBrokerClientResolver brokerClientResolver) : IBrokerTradeExecutor
{
    public async Task<Result<BrokerOrderDto>> ExecuteApprovedTradeAsync(TradeApprovalDto approval, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(approval);

        Result<IBrokerClient> clientResult = brokerClientResolver.Resolve(approval.BrokerProvider);

        if (clientResult.IsFailure)
            return Result<BrokerOrderDto>.Failure(clientResult.Error);

        BrokerOrderRequestDto orderRequest = new()
        {
            Symbol = approval.Symbol,
            Side = approval.Action,
            Quantity = approval.Quantity,
            OrderType = "market",

            // Ties the broker order back to the approval that authorized it, and makes a
            // retried ApproveAsync call idempotent at the broker rather than risking a
            // second live order for the same approval.
            ClientOrderId = approval.Id.ToString("N")
        };

        return await clientResult.Value.PlaceOrderAsync(orderRequest, cancellationToken);
    }
}
