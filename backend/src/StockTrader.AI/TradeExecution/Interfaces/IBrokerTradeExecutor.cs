using StockTrader.Application.Approvals.Dtos;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.TradeExecution.Interfaces;

/// <summary>
/// Places a real broker order for a TradeApproval a human has already approved. This is
/// the only place in the codebase that is allowed to call IBrokerClient.PlaceOrderAsync -
/// TradeApprovalService calls it exactly once, from ApproveAsync, and never before a
/// human decision exists.
/// </summary>
public interface IBrokerTradeExecutor
{
    Task<Result<BrokerOrderDto>> ExecuteApprovedTradeAsync(TradeApprovalDto approval, CancellationToken cancellationToken = default);
}
