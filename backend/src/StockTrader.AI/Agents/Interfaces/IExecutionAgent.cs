using StockTrader.Application.AI.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces;

/// <summary>
/// Bridges an AI trading decision to an actual (paper/simulated) trade.
/// </summary>
public interface IExecutionAgent
{
    Task<Result<ExecutionResultDto>> ExecuteAsync(
        TradingDecisionDto decision,
        CancellationToken cancellationToken = default);
}
