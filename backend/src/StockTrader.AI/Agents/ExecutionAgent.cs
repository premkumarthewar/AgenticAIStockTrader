using StockTrader.AI.Agents.Interfaces;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents;

/// <summary>
/// Turns an AI trading decision into an actual (paper/simulated) trade. This agent
/// trusts that risk evaluation has already happened upstream - TradingAdvisorService
/// overrides a disapproved BUY to HOLD before this agent ever sees the decision - so it
/// focuses only on pricing and placing the trade, not re-running risk assessment.
/// </summary>
public sealed class ExecutionAgent(
    IPaperTradingService paperTradingService,
    IStockMarketService stockMarketService) : IExecutionAgent
{
    public async Task<Result<ExecutionResultDto>> ExecuteAsync(
        TradingDecisionDto decision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);

        string action = decision.Decision.Trim().ToUpperInvariant();

        if (action == "HOLD")
            return NotExecuted(decision.Symbol, action, "Decision was HOLD; no trade was placed.");

        if (action is not ("BUY" or "SELL"))
            return NotExecuted(decision.Symbol, action, $"Unrecognized decision '{decision.Decision}'; no trade was placed.");

        int quantity = (int)decision.RecommendedQuantity;

        if (quantity <= 0)
            return NotExecuted(decision.Symbol, action, "No recommended quantity was provided; no trade was placed.");

        decimal? targetPrice = action == "BUY" ? decision.TargetBuyPrice : decision.TargetSellPrice;

        decimal price;

        if (targetPrice.HasValue && targetPrice.Value > 0)
        {
            price = targetPrice.Value;
        }
        else
        {
            Result<StockQuoteDto> quoteResult = await stockMarketService.GetQuoteAsync(decision.Symbol, cancellationToken);

            if (quoteResult.IsFailure)
                return NotExecuted(decision.Symbol, action,
                    $"No target price was available and the current quote could not be retrieved: {quoteResult.Error.Message}");

            price = quoteResult.Value.CurrentPrice;
        }

        if (price <= 0)
            return NotExecuted(decision.Symbol, action, "No valid execution price could be determined; no trade was placed.");

        PaperTradeRequestDto tradeRequest = new()
        {
            Symbol = decision.Symbol,
            Action = action,
            Quantity = quantity,
            Price = price
        };

        Result<PaperPortfolioDto> tradeResult = await paperTradingService.ExecuteTradeAsync(tradeRequest, cancellationToken);

        if (tradeResult.IsFailure)
            return NotExecuted(decision.Symbol, action, $"Trade could not be executed: {tradeResult.Error.Message}");

        return Result<ExecutionResultDto>.Success(new ExecutionResultDto
        {
            Symbol = decision.Symbol,
            Decision = action,
            Executed = true,
            Reason = "Trade executed via the automated decision pipeline.",
            Portfolio = tradeResult.Value
        });
    }

    private static Result<ExecutionResultDto> NotExecuted(string symbol, string decision, string reason)
    {
        return Result<ExecutionResultDto>.Success(new ExecutionResultDto
        {
            Symbol = symbol,
            Decision = decision,
            Executed = false,
            Reason = reason
        });
    }
}
