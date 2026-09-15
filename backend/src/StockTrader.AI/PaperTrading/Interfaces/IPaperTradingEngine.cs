using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.PaperTrading.Interfaces;

public interface IPaperTradingEngine
{
    Task<Result<PaperPortfolioDto>> ExecuteTradeAsync(
        PaperTradeRequestDto request,
        CancellationToken cancellationToken = default);

    Task<Result<PaperPortfolioDto>> GetPortfolioAsync(
        CancellationToken cancellationToken = default);

    Task<Result<PaperPositionDto>> GetPositionAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
