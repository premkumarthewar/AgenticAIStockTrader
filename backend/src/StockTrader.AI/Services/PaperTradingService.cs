using StockTrader.AI.PaperTrading.Interfaces;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Services;

public sealed class PaperTradingService(IPaperTradingEngine paperTradingEngine, IPaperTradingPersistence persistence) : IPaperTradingService
{
    public Task<Result<PaperPortfolioDto>> ExecuteTradeAsync(Guid userId, PaperTradeRequestDto request, CancellationToken cancellationToken = default)
    {
        return paperTradingEngine.ExecuteTradeAsync(userId, request, cancellationToken);
    }

    public Task<Result<PaperPortfolioDto>> GetPortfolioAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return paperTradingEngine.GetPortfolioAsync(userId, cancellationToken);
    }

    public Task<Result<PaperPositionDto>> GetPositionAsync(
        Guid userId, string symbol, CancellationToken cancellationToken = default)
    {
        return paperTradingEngine.GetPositionAsync(userId, symbol, cancellationToken);
    }

    public async Task<Result<PaperPortfolioDto>> InitializePortfolioAsync(Guid userId, decimal initialCapital, CancellationToken cancellationToken = default)
    {
        if (initialCapital <= 0)
        {
            return Result<PaperPortfolioDto>.Failure(new Error("BadRequest", "Initial capital must be greater than zero."));
        }

        PaperPortfolio? existingPortfolio = await persistence.GetPortfolioAsync(userId, cancellationToken);

        if (existingPortfolio is not null)
            return Result<PaperPortfolioDto>.Failure(new Error("InternalServerError", "This user's paper trading portfolio has already been initialized."));

        DateTime now = DateTime.UtcNow;

        PaperPortfolio portfolio = new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            InitialCapital = initialCapital,
            CashBalance = initialCapital,
            CreatedOnUtc = now,
            LastUpdatedOnUtc = now
        };

        await persistence.AddPortfolioAsync(portfolio, cancellationToken);

        await persistence.SaveChangesAsync(cancellationToken);

        return Result<PaperPortfolioDto>.Success(
            new PaperPortfolioDto
            {
                InitialCapital = initialCapital,
                CashBalance = initialCapital,
                InvestedValue = 0,
                CurrentPortfolioValue = initialCapital,
                ProfitLoss = 0,
                ReturnPercentage = 0,
                Positions = [],
                Trades = []
            });
    }
}
