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

    public static decimal CalculateMaxDrawdown(IReadOnlyList<SimulatedTradeDto> trades)
    {
        if (trades.Count == 0)
            return 0;

        decimal peak = trades[0].PortfolioValue;

        decimal maxDrawdown = 0;

        foreach (SimulatedTradeDto trade in trades)
        {
            if (trade.PortfolioValue > peak)
                peak = trade.PortfolioValue;

            decimal drawdown = peak == 0 ? 0 : ((peak - trade.PortfolioValue) / peak) * 100m;

            if (drawdown > maxDrawdown)
                maxDrawdown = drawdown;
        }

        return maxDrawdown;
    }
}