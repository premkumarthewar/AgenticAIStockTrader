using StockTrader.Application.MarketData.Dtos;

namespace StockTrader.AI.Backtesting.Interfaces;

/// <summary>
/// A deterministic, rule-based trading strategy used only for backtesting. Unlike the
/// live AI agent pipeline (ITradingAdvisorService), a strategy here looks only at price
/// history that was actually available up to and including the day being evaluated, so
/// a backtest run is fast, free of API cost, and point-in-time correct. The live and
/// paper-trading paths are unaffected and keep using the full multi-agent AI pipeline.
/// </summary>
public interface IBacktestStrategy
{
    /// <summary>
    /// Decides an action for the trading day at <paramref name="currentIndex"/> within
    /// <paramref name="priceHistory"/>, using only price data up to and including that
    /// index. Implementations must never look ahead of <paramref name="currentIndex"/>.
    /// </summary>
    BacktestSignal Decide(IReadOnlyList<HistoricalPriceDto> priceHistory, int currentIndex);
}

public enum BacktestSignal
{
    Hold,
    Buy,
    Sell
}
