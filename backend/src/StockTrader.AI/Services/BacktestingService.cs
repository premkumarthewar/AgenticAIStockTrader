using StockTrader.AI.Backtesting.Interfaces;
using StockTrader.Application.Backtesting.Dtos;
using StockTrader.Application.Backtesting.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Services;

public sealed class BacktestingService(
    IBacktestingEngine engine) : IBacktestingService
{
    public async Task<Result<BacktestResultDto>> RunAsync(
        BacktestRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await engine.ExecuteAsync(request, cancellationToken);
    }
}