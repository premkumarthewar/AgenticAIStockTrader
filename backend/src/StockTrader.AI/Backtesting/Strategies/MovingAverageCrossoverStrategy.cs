using StockTrader.AI.Backtesting.Interfaces;
using StockTrader.Application.MarketData.Dtos;

namespace StockTrader.AI.Backtesting.Strategies;

/// <summary>
/// Classic moving-average crossover strategy: BUY when the short-term simple moving
/// average crosses above the long-term simple moving average (a "golden cross"), SELL
/// when it crosses back below (a "death cross"), HOLD otherwise. Chosen as the default
/// backtesting strategy because it is deterministic and needs nothing beyond the
/// historical closing prices already loaded for the backtest, so it runs instantly and
/// costs nothing, unlike a per-day call into the AI agent pipeline.
/// </summary>
public sealed class MovingAverageCrossoverStrategy(int shortWindow = 10, int longWindow = 30) : IBacktestStrategy
{
    private readonly int shortWindow = shortWindow > 0 ? shortWindow : 10;
    private readonly int longWindow = longWindow > shortWindow ? longWindow : shortWindow + 1;

    public BacktestSignal Decide(IReadOnlyList<HistoricalPriceDto> priceHistory, int currentIndex)
    {
        // Need a full "long" window of history for both today and yesterday before a
        // crossover can be detected, otherwise there isn't enough data yet.
        if (currentIndex < longWindow)
            return BacktestSignal.Hold;

        decimal currentShortAverage = CalculateSimpleMovingAverage(priceHistory, currentIndex, shortWindow);
        decimal currentLongAverage = CalculateSimpleMovingAverage(priceHistory, currentIndex, longWindow);

        decimal previousShortAverage = CalculateSimpleMovingAverage(priceHistory, currentIndex - 1, shortWindow);
        decimal previousLongAverage = CalculateSimpleMovingAverage(priceHistory, currentIndex - 1, longWindow);

        bool crossedAbove =
            previousShortAverage <= previousLongAverage &&
            currentShortAverage > currentLongAverage;

        bool crossedBelow =
            previousShortAverage >= previousLongAverage &&
            currentShortAverage < currentLongAverage;

        if (crossedAbove)
            return BacktestSignal.Buy;

        if (crossedBelow)
            return BacktestSignal.Sell;

        return BacktestSignal.Hold;
    }

    private static decimal CalculateSimpleMovingAverage(IReadOnlyList<HistoricalPriceDto> priceHistory, int endIndex, int window)
    {
        decimal sum = 0m;

        for (int i = endIndex - window + 1; i <= endIndex; i++)
            sum += priceHistory[i].Close;

        return sum / window;
    }
}
