using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces;

/// <summary>
/// Provides AI-powered market analysis and decision-making capabilities for stock trading.
/// </summary>
public interface IMarketAgent
{
    /// <summary>
    /// Analyzes the specified stock using available market intelligence tools.
    /// </summary>
    /// <param name="symbol">The stock ticker symbol.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The market analysis result.</returns>
    Task<Result<string>> AnalyzeAsync(AnalyzeStockRequest request, CancellationToken cancellationToken = default);
}
