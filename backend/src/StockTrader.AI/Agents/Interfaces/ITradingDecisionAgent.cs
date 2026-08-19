using StockTrader.Application.AI.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces
{
    public interface ITradingDecisionAgent
    {
        Task<Result<TradingDecisionDto>> DecideAsync(string symbol, string integratedAnalysis, CancellationToken cancellationToken = default);
    }
}
