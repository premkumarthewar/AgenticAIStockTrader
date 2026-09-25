using StockTrader.Application.PaperTrading.Dtos;

namespace StockTrader.Application.AI.Dtos;

/// <summary>
/// The outcome of handing an AI trading decision to ExecutionAgent. Executed is false
/// for every legitimate reason a trade wasn't placed (HOLD decision, no quantity, no
/// price available, insufficient cash/holdings), as well as for a hard system failure -
/// Reason always explains which.
/// </summary>
public sealed record ExecutionResultDto
{
    public required string Symbol { get; init; }

    public required string Decision { get; init; }

    public required bool Executed { get; init; }

    public required string Reason { get; init; }

    public PaperPortfolioDto? Portfolio { get; init; }
}
