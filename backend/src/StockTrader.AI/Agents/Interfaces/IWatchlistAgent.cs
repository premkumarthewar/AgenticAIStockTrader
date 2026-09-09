using StockTrader.Application.AI.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces;

public interface IWatchlistAgent
{
    Task<Result<WatchlistAnalysisDto>> AnalyzeAsync(WatchlistDto request, CancellationToken cancellationToken = default);
}
