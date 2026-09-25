using StockTrader.AI.Backtesting.Interfaces;
using StockTrader.Application.Backtesting.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Backtesting;

public sealed class BacktestingEngine(
    IStockMarketService stockMarketService,
    IBacktestStrategy strategy) : IBacktestingEngine
{
    public async Task<Result<BacktestResultDto>> ExecuteAsync(BacktestRequestDto request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return Result<BacktestResultDto>.Failure(
                new Error("BadRequest", "Stock symbol is required."));

        if (request.InitialCapital <= 0)
            return Result<BacktestResultDto>.Failure(
                new Error("BadRequest", "Initial capital must be greater than zero."));

        if (request.CommissionPerTrade < 0)
            return Result<BacktestResultDto>.Failure(
                new Error("BadRequest", "Commission per trade cannot be negative."));

        if (request.StartDate > request.EndDate)
            return Result<BacktestResultDto>.Failure(
                new Error("BadRequest", "Start date cannot be later than end date."));

        string symbol = request.Symbol.Trim().ToUpperInvariant();

        Result<IReadOnlyList<HistoricalPriceDto>> historicalResult = await stockMarketService
                .GetHistoricalPricesAsync(symbol,
                    request.StartDate.ToDateTime(TimeOnly.MinValue),
                    request.EndDate.ToDateTime(TimeOnly.MinValue),
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

        List<EquityPointDto> equityCurve = new(prices.Count);

        // Walks the historical prices once, in order, asking the (deterministic,
        // point-in-time-correct) strategy for a decision at each day using only data
        // available up to and including that day - no look-ahead, and no per-day call
        // into the live AI agent pipeline.
        for (int i = 0; i < prices.Count; i++)
        {
            HistoricalPriceDto price = prices[i];

            BacktestSignal signal = strategy.Decide(prices, i);

            ExecuteTrade(
                signal,
                price,
                symbol,
                request.CommissionPerTrade,
                trades,
                ref cash,
                ref sharesOwned);

            equityCurve.Add(new EquityPointDto
            {
                Date = price.Date,
                PortfolioValue = cash + (sharesOwned * price.Close)
            });
        }

        decimal finalPortfolioValue = cash + (sharesOwned * prices[prices.Count - 1].Close);

        decimal totalReturn = BacktestMetricsCalculator.CalculateReturn(
            request.InitialCapital, finalPortfolioValue);

        decimal cagr = BacktestMetricsCalculator.CalculateCagr(
            request.InitialCapital, finalPortfolioValue, request.StartDate, request.EndDate);

        decimal maxDrawdown = BacktestMetricsCalculator.CalculateMaxDrawdown(equityCurve);

        decimal sharpeRatio = BacktestMetricsCalculator.CalculateSharpeRatio(equityCurve);

        var (WinningTrades, LosingTrades, WinRate, ProfitFactor) =
            BacktestMetricsCalculator.CalculateTradeStatistics(trades);

        BacktestResultDto result =
     new()
     {
         InitialCapital = request.InitialCapital,
         FinalPortfolioValue = finalPortfolioValue,
         TotalProfitLoss = finalPortfolioValue - request.InitialCapital,
         TotalReturnPercentage = totalReturn,
         CAGR = cagr,
         MaxDrawdown = maxDrawdown,
         SharpeRatio = sharpeRatio,
         WinRate = WinRate,
         ProfitFactor = ProfitFactor,
         WinningTrades = WinningTrades,
         LosingTrades = LosingTrades,
         TotalTrades = trades.Count,
         Trades = trades,
         EquityCurve = equityCurve
     };

        return Result<BacktestResultDto>.Success(
            result);
    }

    private static void ExecuteTrade(
        BacktestSignal signal,
        HistoricalPriceDto price,
        string symbol,
        decimal commissionPerTrade,
        List<SimulatedTradeDto> trades,
        ref decimal cash,
        ref int sharesOwned)
    {
        decimal tradePrice = price.Close;

        if (signal == BacktestSignal.Buy)
        {
            int quantity = (int)((cash - commissionPerTrade) / tradePrice);

            if (quantity <= 0)
                return;

            decimal cost = (quantity * tradePrice) + commissionPerTrade;

            cash -= cost;

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

            return;
        }

        if (signal == BacktestSignal.Sell && sharesOwned > 0)
        {
            decimal proceeds = (sharesOwned * tradePrice) - commissionPerTrade;

            cash += proceeds;

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
}
