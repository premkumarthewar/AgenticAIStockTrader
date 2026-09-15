using StockTrader.Application.Backtesting.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Backtesting.Interfaces;

public interface IBacktestingEngine
{
    Task<Result<BacktestResultDto>> ExecuteAsync(
        BacktestRequestDto request,
        CancellationToken cancellationToken = default);
}
