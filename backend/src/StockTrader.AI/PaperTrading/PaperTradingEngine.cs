using StockTrader.AI.PaperTrading.Interfaces;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;

namespace StockTrader.AI.PaperTrading;

public sealed class PaperTradingEngine(
    IPaperTradingPersistence persistence,
    IStockMarketService stockMarketService) : IPaperTradingEngine
{
    public async Task<Result<PaperPortfolioDto>> ExecuteTradeAsync(
        PaperTradeRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return Result<PaperPortfolioDto>.Failure(new Error("BadRequest", "Trade request is required."));

        string symbol = request.Symbol.Trim().ToUpperInvariant();

        string action = request.Action.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(symbol))
            return Result<PaperPortfolioDto>.Failure(new Error("BadRequest", "Symbol is required."));

        if (action is not ("BUY" or "SELL"))
            return Result<PaperPortfolioDto>.Failure(new Error("BadRequest", "Action must be BUY or SELL."));

        if (request.Quantity <= 0)
            return Result<PaperPortfolioDto>.Failure(new Error("BadRequest", "Quantity must be greater than zero."));

        if (request.Price <= 0)
            return Result<PaperPortfolioDto>.Failure(new Error("BadRequest", "Price must be greater than zero."));

        PaperPortfolio? portfolio =
            await persistence.GetPortfolioAsync(cancellationToken);

        if (portfolio is null)
        {
            return Result<PaperPortfolioDto>.Failure(new Error("InternalServerError", "Paper trading portfolio has not been initialized."));
        }

        decimal tradeValue = request.Quantity * request.Price;

        if (action == "BUY")
        {
            if (portfolio.CashBalance < tradeValue)
                return Result<PaperPortfolioDto>.Failure(new Error("InternalServerError",
                    $"Insufficient cash balance. " +
                    $"Required: {tradeValue:F2}, " +
                    $"Available: {portfolio.CashBalance:F2}."));

            PaperPosition? position =
                portfolio.Positions.FirstOrDefault(
                    x => x.Symbol == symbol);

            if (position is null)
            {
                position = new PaperPosition
                {
                    Id = Guid.NewGuid(),
                    PaperPortfolioId = portfolio.Id,
                    Symbol = symbol,
                    Quantity = request.Quantity,
                    AveragePrice = request.Price,
                    LastUpdatedOnUtc = DateTime.UtcNow
                };

                portfolio.Positions.Add(position);
            }
            else
            {
                decimal existingCost =
                    position.Quantity * position.AveragePrice;

                decimal newCost =
                    request.Quantity * request.Price;

                int newQuantity =
                    position.Quantity + request.Quantity;

                position.AveragePrice =
                    (existingCost + newCost) / newQuantity;

                position.Quantity = newQuantity;
                position.LastUpdatedOnUtc = DateTime.UtcNow;
            }

            portfolio.CashBalance -= tradeValue;
        }
        else
        {
            PaperPosition? position =
                portfolio.Positions.FirstOrDefault(
                    x => x.Symbol == symbol);

            if (position is null ||
                position.Quantity < request.Quantity)
            {
                decimal availableQuantity =
                    position?.Quantity ?? 0;

                return Result<PaperPortfolioDto>.Failure(new Error("InternalServerError",
                    $"Insufficient holdings for {symbol}. " +
                    $"Requested: {request.Quantity}, " +
                    $"Available: {availableQuantity}."));
            }

            position.Quantity -= request.Quantity;

            portfolio.CashBalance += tradeValue;

            if (position.Quantity == 0)
                portfolio.Positions.Remove(position);
            else
                position.LastUpdatedOnUtc = DateTime.UtcNow;
        }

        portfolio.LastUpdatedOnUtc = DateTime.UtcNow;

        PaperTrade trade = new()
        {
            Id = Guid.NewGuid(),
            PaperPortfolioId = portfolio.Id,
            Symbol = symbol,
            Action = action,
            Quantity = request.Quantity,
            Price = request.Price,
            TotalValue = tradeValue,
            ExecutedAtUtc = DateTime.UtcNow
        };

        await persistence.AddTradeAsync(
            trade,
            cancellationToken);

        await persistence.SaveChangesAsync(
            cancellationToken);

        return await BuildPortfolioResultAsync(
            portfolio,
            cancellationToken);
    }

    public async Task<Result<PaperPortfolioDto>> GetPortfolioAsync(
        CancellationToken cancellationToken = default)
    {
        PaperPortfolio? portfolio = await persistence.GetPortfolioAsync(cancellationToken);

        if (portfolio is null)
        {
            return Result<PaperPortfolioDto>.Failure(new Error("InternalServerError",
                "Paper trading portfolio has not been initialized."));
        }

        return await BuildPortfolioResultAsync(
            portfolio,
            cancellationToken);
    }

    public async Task<Result<PaperPositionDto>> GetPositionAsync(string symbol, CancellationToken cancellationToken = default)
    {
        string normalizedSymbol = symbol.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedSymbol))
            return Result<PaperPositionDto>.Failure(new Error("BadRequest", "Symbol is required."));

        PaperPortfolio? portfolio = await persistence.GetPortfolioAsync(cancellationToken);

        if (portfolio is null)
            return Result<PaperPositionDto>.Failure(new Error("InternalServerError", "Paper trading portfolio has not been initialized."));

        PaperPosition? position = portfolio.Positions.FirstOrDefault(x => x.Symbol == normalizedSymbol);

        if (position is null)
        {
            return Result<PaperPositionDto>.Failure(new Error("InternalServerError", $"No paper position exists for {normalizedSymbol}."));
        }

        Result<StockQuoteDto> quoteResult =
            await stockMarketService.GetQuoteAsync(
                normalizedSymbol,
                cancellationToken);

        if (quoteResult.IsFailure)
        {
            return Result<PaperPositionDto>.Failure(
                quoteResult.Error);
        }

        decimal currentPrice =
            quoteResult.Value.CurrentPrice;

        decimal marketValue =
            position.Quantity * currentPrice;

        decimal investedValue =
            position.Quantity * position.AveragePrice;

        decimal unrealizedProfitLoss =
            marketValue - investedValue;

        decimal unrealizedReturn =
            investedValue == 0
                ? 0
                : unrealizedProfitLoss /
                  investedValue *
                  100;

        return Result<PaperPositionDto>.Success(
            new PaperPositionDto
            {
                Symbol = position.Symbol,
                Quantity = position.Quantity,
                AveragePrice = position.AveragePrice,
                CurrentPrice = currentPrice,
                MarketValue = marketValue,
                UnrealizedProfitLoss =
                    unrealizedProfitLoss,
                UnrealizedReturnPercentage =
                    unrealizedReturn
            });
    }

    private async Task<Result<PaperPortfolioDto>> BuildPortfolioResultAsync(PaperPortfolio portfolio, CancellationToken cancellationToken)
    {
        List<PaperPositionDto> positions = [];

        decimal investedValue = 0;
        decimal currentMarketValue = 0;

        foreach (PaperPosition position in portfolio.Positions)
        {
            Result<StockQuoteDto> quoteResult = await stockMarketService.GetQuoteAsync(position.Symbol, cancellationToken);

            if (quoteResult.IsFailure)
                return Result<PaperPortfolioDto>.Failure(new Error("InternalServerError",
                    $"Unable to retrieve current price for " +
                    $"{position.Symbol}: {quoteResult.Error}"));

            decimal currentPrice =
                quoteResult.Value.CurrentPrice;

            decimal marketValue =
                position.Quantity * currentPrice;

            decimal positionCost =
                position.Quantity * position.AveragePrice;

            decimal unrealizedProfitLoss =
                marketValue - positionCost;

            decimal unrealizedReturn =
                positionCost == 0
                    ? 0
                    : unrealizedProfitLoss /
                      positionCost *
                      100;

            investedValue += positionCost;
            currentMarketValue += marketValue;

            positions.Add(
                new PaperPositionDto
                {
                    Symbol = position.Symbol,
                    Quantity = position.Quantity,
                    AveragePrice = position.AveragePrice,
                    CurrentPrice = currentPrice,
                    MarketValue = marketValue,
                    UnrealizedProfitLoss =
                        unrealizedProfitLoss,
                    UnrealizedReturnPercentage =
                        unrealizedReturn
                });
        }

        decimal portfolioValue =
            portfolio.CashBalance +
            currentMarketValue;

        decimal profitLoss =
            portfolioValue -
            portfolio.InitialCapital;

        decimal returnPercentage =
            portfolio.InitialCapital == 0
                ? 0
                : profitLoss /
                  portfolio.InitialCapital *
                  100;

        List<PaperTradeDto> trades =
            [.. portfolio.Trades
                .OrderByDescending(x => x.ExecutedAtUtc)
                .Select(
                    x => new PaperTradeDto
                    {
                        Id = x.Id,
                        Symbol = x.Symbol,
                        Action = x.Action,
                        Quantity = x.Quantity,
                        Price = x.Price,
                        TotalValue = x.TotalValue,
                        ExecutedAtUtc = x.ExecutedAtUtc
                    })];

        return Result<PaperPortfolioDto>.Success(
            new PaperPortfolioDto
            {
                InitialCapital =
                    portfolio.InitialCapital,

                CashBalance =
                    portfolio.CashBalance,

                InvestedValue =
                    investedValue,

                CurrentPortfolioValue =
                    portfolioValue,

                ProfitLoss =
                    profitLoss,

                ReturnPercentage =
                    returnPercentage,

                Positions =
                    positions,

                Trades =
                    trades
            });
    }
}