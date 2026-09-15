using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.PaperTrading.Interfaces;

public interface IPaperTradingService
{
    Task<Result<PaperPortfolioDto>> ExecuteTradeAsync(PaperTradeRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<PaperPortfolioDto>> GetPortfolioAsync(CancellationToken cancellationToken = default);

    Task<Result<PaperPositionDto>> GetPositionAsync(string symbol, CancellationToken cancellationToken = default);

    Task<Result<PaperPortfolioDto>> InitializePortfolioAsync(decimal initialCapital, CancellationToken cancellationToken = default);
}
