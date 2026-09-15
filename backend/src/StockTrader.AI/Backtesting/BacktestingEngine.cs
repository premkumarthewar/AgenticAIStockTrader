using StockTrader.AI.Backtesting.Interfaces;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Backtesting.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Backtesting;

public sealed class BacktestingEngine(
    IStockMarketService stockMarketService,
    ITradingAdvisorService tradingAdvisorService) : IBacktestingEngine
{
    public async Task<Result<BacktestResultDto>> ExecuteAsync(BacktestRequestDto request, CancellationToken cancellationToken = default)
    {
        string symbol = request.Symbol.Trim().ToUpperInvariant();

        Result<IReadOnlyList<HistoricalPriceDto>> historicalResult = await stockMarketService
                .GetHistoricalPricesAsync(symbol,
                    Convert.ToDateTime(request.StartDate),
                    Convert.ToDateTime(request.EndDate),
                    cancellationToken);

        if (historicalResult.IsFailure)
            return Result<BacktestResultDto>.Failure(
                historicalResult.Error);

        IReadOnlyList<HistoricalPriceDto> prices =
            historicalResult.Value;

        if (prices.Count == 0)
            return Result<BacktestResultDto>.Failure(
                new Error("NotFound", "No historical data available."));

        decimal cash = request.InitialCapital;

        int sharesOwned = 0;

        List<SimulatedTradeDto> trades = [];

        foreach (HistoricalPriceDto price in prices)
        {
            Result<TradingDecisionDto> decisionResult =
                await tradingAdvisorService.AnalyzeAsync(new AnalyzeStockRequest { Symbol = symbol }, cancellationToken);

            if (decisionResult.IsFailure)
                continue;

            TradingDecisionDto decision = decisionResult.Value;

            await ExecuteTradeAsync(decision, price, symbol,
                trades, ref cash, ref sharesOwned);
        }

        decimal finalPortfolioValue = cash + (sharesOwned * prices[prices.Count - 1].Close);

        decimal totalReturn = BacktestMetricsCalculator.CalculateReturn(
        request.InitialCapital, finalPortfolioValue);

        decimal cagr = BacktestMetricsCalculator.CalculateCagr(request.InitialCapital, finalPortfolioValue, request.StartDate, request.EndDate);

        decimal maxDrawdown = BacktestMetricsCalculator.CalculateMaxDrawdown(trades);

        var (WinningTrades, LosingTrades, WinRate, ProfitFactor) = CalculateTradeStatistics(trades);

        BacktestResultDto result =
     new()
     {
         InitialCapital = request.InitialCapital,
         FinalPortfolioValue = finalPortfolioValue,
         TotalProfitLoss = finalPortfolioValue - request.InitialCapital,
         TotalReturnPercentage = totalReturn,
         CAGR = cagr,
         MaxDrawdown = maxDrawdown,
         WinRate = WinRate,
         ProfitFactor = ProfitFactor,
         WinningTrades = WinningTrades,
         LosingTrades = LosingTrades,
         TotalTrades = trades.Count,
         Trades = trades
     };

        return Result<BacktestResultDto>.Success(
            result);
    }

    private static Task ExecuteTradeAsync(TradingDecisionDto decision, HistoricalPriceDto price, string symbol, List<SimulatedTradeDto> trades, ref decimal cash, ref int sharesOwned)
    {
        string action = decision.Decision.Trim().ToUpperInvariant();

        decimal tradePrice = price.Close;

        if (action == "BUY")
        {
            int quantity = (int)(cash / tradePrice);

            if (quantity > 0)
            {
                cash -= quantity * tradePrice;

                sharesOwned += quantity;

                trades.Add(new SimulatedTradeDto
                {
                    Symbol = symbol,
                    Action = "BUY",
                    ExecutedAt = price.Date,
                    Price = tradePrice,
                    Quantity = quantity,
                    CashBalance = cash,
                    PortfolioValue = cash + (sharesOwned * tradePrice)
                });
            }
        }

        if (action == "SELL")
        {
            if (sharesOwned > 0)
            {
                cash += sharesOwned * tradePrice;

                trades.Add(new SimulatedTradeDto
                {
                    Symbol = symbol,
                    Action = "SELL",
                    ExecutedAt = price.Date,
                    Price = tradePrice,
                    Quantity = sharesOwned,
                    CashBalance = cash,
                    PortfolioValue = cash
                });

                sharesOwned = 0;
            }
        }

        return Task.CompletedTask;
    }

    private static (int WinningTrades, int LosingTrades, decimal WinRate, decimal ProfitFactor) CalculateTradeStatistics(IReadOnlyList<SimulatedTradeDto> trades)
    {
        int wins = 0;
        int losses = 0;

        decimal grossProfit = 0;
        decimal grossLoss = 0;

        for (int i = 1; i < trades.Count; i++)
        {
            SimulatedTradeDto previous =
                trades[i - 1];

            SimulatedTradeDto current =
                trades[i];

            if (previous.Action != "BUY" ||
                current.Action != "SELL")
                continue;

            decimal pnl = (current.Price - previous.Price) * previous.Quantity;

            if (pnl >= 0)
            {
                wins++;
                grossProfit += pnl;
            }
            else
            {
                losses++;
                grossLoss += Math.Abs(pnl);
            }
        }

        int totalTrades = wins + losses;

        decimal winRate = totalTrades == 0 ? 0 : (decimal)wins / totalTrades * 100m;

        decimal profitFactor = grossLoss == 0 ? grossProfit
            : grossProfit / grossLoss;

        return (wins, losses, winRate, profitFactor);
    }
}