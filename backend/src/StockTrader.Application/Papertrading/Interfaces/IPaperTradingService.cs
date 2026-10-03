using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.PaperTrading.Interfaces;

public interface IPaperTradingService
{
    Task<Result<PaperPortfolioDto>> ExecuteTradeAsync(Guid userId, PaperTradeRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<PaperPortfolioDto>> GetPortfolioAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<PaperPositionDto>> GetPositionAsync(Guid userId, string symbol, CancellationToken cancellationToken = default);

    Task<Result<PaperPortfolioDto>> InitializePortfolioAsync(Guid userId, decimal initialCapital, CancellationToken cancellationToken = default);
}
