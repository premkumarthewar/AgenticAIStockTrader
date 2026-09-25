using StockTrader.Application.Backtesting.Dtos;

namespace StockTrader.AI.Backtesting;

internal static class BacktestMetricsCalculator
{
    public static decimal CalculateReturn(decimal initialCapital, decimal finalValue)
    {
        if (initialCapital == 0)
            return 0;

        return
            ((finalValue - initialCapital)
            / initialCapital) * 100m;
    }

    public static decimal CalculateCagr(decimal initialCapital, decimal finalValue, DateOnly startDate, DateOnly endDate)
    {
        if (initialCapital <= 0)
            return 0;

        double years = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25;

        if (years <= 0)
            return 0;

        double result = Math.Pow((double)(finalValue / initialCapital), 1d / years) - 1d;

        return (decimal)(result * 100d);
    }

    /// <summary>
    /// Maximum peak-to-trough decline of the portfolio's daily mark-to-market value,
    /// as a percentage. Using the full daily equity curve (rather than only the values
    /// recorded at trade events) catches drawdowns that happen while a position is
    /// simply being held.
    /// </summary>
    public static decimal CalculateMaxDrawdown(IReadOnlyList<EquityPointDto> equityCurve)
    {
        if (equityCurve.Count == 0)
            return 0;

        decimal peak = equityCurve[0].PortfolioValue;

        decimal maxDrawdown = 0;

        foreach (EquityPointDto point in equityCurve)
        {
            if (point.PortfolioValue > peak)
                peak = point.PortfolioValue;

            decimal drawdown = peak == 0 ? 0 : ((peak - point.PortfolioValue) / peak) * 100m;

            if (drawdown > maxDrawdown)
                maxDrawdown = drawdown;
        }

        return maxDrawdown;
    }

    /// <summary>
    /// Annualized Sharpe ratio computed from day-over-day returns on the equity curve,
    /// assuming a 0% risk-free rate and 252 trading days per year.
    /// </summary>
    public static decimal CalculateSharpeRatio(IReadOnlyList<EquityPointDto> equityCurve)
    {
        if (equityCurve.Count < 2)
            return 0;

        List<double> dailyReturns = new(equityCurve.Count - 1);

        for (int i = 1; i < equityCurve.Count; i++)
        {
            decimal previousValue = equityCurve[i - 1].PortfolioValue;

            if (previousValue == 0)
                continue;

            double dailyReturn = (double)((equityCurve[i].PortfolioValue - previousValue) / previousValue);

            dailyReturns.Add(dailyReturn);
        }

        if (dailyReturns.Count < 2)
            return 0;

        double meanReturn = dailyReturns.Average();

        double sumOfSquaredDeviations = dailyReturns.Sum(r => Math.Pow(r - meanReturn, 2));

        double standardDeviation = Math.Sqrt(sumOfSquaredDeviations / (dailyReturns.Count - 1));

        if (standardDeviation == 0)
            return 0;

        double annualizedSharpeRatio = (meanReturn / standardDeviation) * Math.Sqrt(252);

        return (decimal)annualizedSharpeRatio;
    }

    /// <summary>
    /// Win rate and profit factor computed by tracking the open position's weighted
    /// average cost basis as BUY trades accumulate shares, then realizing profit/loss
    /// against that basis when a SELL liquidates the position. This is required
    /// because a SELL always liquidates the full open position (see
    /// BacktestingEngine.ExecuteTrade), so more than one BUY can precede a single
    /// SELL; pairing trades by adjacent list position instead of by cost basis would
    /// misattribute P&amp;L whenever that happens.
    /// </summary>
    public static (int WinningTrades, int LosingTrades, decimal WinRate, decimal ProfitFactor) CalculateTradeStatistics(IReadOnlyList<SimulatedTradeDto> trades)
    {
        int wins = 0;
        int losses = 0;

        decimal grossProfit = 0;
        decimal grossLoss = 0;

        decimal openShares = 0;
        decimal openCostBasis = 0;

        foreach (SimulatedTradeDto trade in trades)
        {
            if (trade.Action == "BUY")
            {
                openShares += trade.Quantity;
                openCostBasis += trade.Quantity * trade.Price;

                continue;
            }

            if (trade.Action != "SELL" || openShares <= 0)
                continue;

            decimal proceeds = trade.Quantity * trade.Price;

            decimal pnl = proceeds - openCostBasis;

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

            openShares = 0;
            openCostBasis = 0;
        }

        int totalTrades = wins + losses;

        decimal winRate = totalTrades == 0 ? 0 : (decimal)wins / totalTrades * 100m;

        decimal profitFactor = grossLoss == 0 ? grossProfit
            : grossProfit / grossLoss;

        return (wins, losses, winRate, profitFactor);
    }
}
