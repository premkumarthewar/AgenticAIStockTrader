using StockTrader.Application.Backtesting.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.Backtesting.Interfaces;

public interface IBacktestingService
{
    Task<Result<BacktestResultDto>> RunAsync(
        BacktestRequestDto request,
        CancellationToken cancellationToken = default);
}
