using Moq;
using StockTrader.AI.PaperTrading;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;

namespace StockTrader.UnitTests.AI;

public class PaperTradingEngineTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static StockQuoteDto Quote(string symbol, decimal currentPrice) => new()
    {
        Symbol = symbol,
        CurrentPrice = currentPrice
    };

    private static (Mock<IPaperTradingPersistence> Persistence, Mock<IStockMarketService> MarketData, PaperTradingEngine Engine) CreateSut(PaperPortfolio? portfolio)
    {
        Mock<IPaperTradingPersistence> persistence = new();

        persistence
            .Setup(x => x.GetPortfolioAsync(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(portfolio);

        Mock<IStockMarketService> marketData = new();

        PaperTradingEngine engine = new(persistence.Object, marketData.Object);

        return (persistence, marketData, engine);
    }

    [Fact]
    public async Task ExecuteTradeAsync_BuyWithSufficientCash_DebitsCashAndOpensAPosition()
    {
        PaperPortfolio portfolio = new()
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            InitialCapital = 10_000m,
            CashBalance = 10_000m
        };

        (Mock<IPaperTradingPersistence> persistence, Mock<IStockMarketService> marketData, PaperTradingEngine engine) = CreateSut(portfolio);

        marketData
            .Setup(x => x.GetQuoteAsync("AAPL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<StockQuoteDto>.Success(Quote("AAPL", 150m)));

        PaperTradeRequestDto request = new() { Symbol = "AAPL", Action = "BUY", Quantity = 10, Price = 150m };

        Result<PaperPortfolioDto> result = await engine.ExecuteTradeAsync(UserId, request);

        Assert.True(result.IsSuccess);
        Assert.Equal(10_000m - 1_500m, portfolio.CashBalance);
        Assert.Single(portfolio.Positions);
        Assert.Equal(10, portfolio.Positions.Single().Quantity);

        persistence.Verify(x => x.AddTradeAsync(It.IsAny<PaperTrade>(), It.IsAny<CancellationToken>()), Times.Once);
        persistence.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteTradeAsync_BuyWithInsufficientCash_FailsAndDoesNotPersistATrade()
    {
        PaperPortfolio portfolio = new()
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            InitialCapital = 100m,
            CashBalance = 100m
        };

        (Mock<IPaperTradingPersistence> persistence, _, PaperTradingEngine engine) = CreateSut(portfolio);

        PaperTradeRequestDto request = new() { Symbol = "AAPL", Action = "BUY", Quantity = 10, Price = 150m };

        Result<PaperPortfolioDto> result = await engine.ExecuteTradeAsync(UserId, request);

        Assert.True(result.IsFailure);
        Assert.Equal(100m, portfolio.CashBalance);

        persistence.Verify(x => x.AddTradeAsync(It.IsAny<PaperTrade>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteTradeAsync_SellMoreThanHeld_Fails()
    {
        PaperPortfolio portfolio = new()
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            InitialCapital = 10_000m,
            CashBalance = 5_000m,
            Positions =
            [
                new PaperPosition { Id = Guid.NewGuid(), Symbol = "AAPL", Quantity = 5, AveragePrice = 140m }
            ]
        };

        (_, _, PaperTradingEngine engine) = CreateSut(portfolio);

        PaperTradeRequestDto request = new() { Symbol = "AAPL", Action = "SELL", Quantity = 10, Price = 150m };

        Result<PaperPortfolioDto> result = await engine.ExecuteTradeAsync(UserId, request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ExecuteTradeAsync_NoPortfolioForUser_Fails()
    {
        (_, _, PaperTradingEngine engine) = CreateSut(portfolio: null);

        PaperTradeRequestDto request = new() { Symbol = "AAPL", Action = "BUY", Quantity = 1, Price = 150m };

        Result<PaperPortfolioDto> result = await engine.ExecuteTradeAsync(UserId, request);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetPortfolioAsync_LooksUpPortfolioScopedToTheGivenUser()
    {
        PaperPortfolio portfolio = new() { Id = Guid.NewGuid(), UserId = UserId, InitialCapital = 1_000m, CashBalance = 1_000m };

        (Mock<IPaperTradingPersistence> persistence, _, PaperTradingEngine engine) = CreateSut(portfolio);

        Result<PaperPortfolioDto> result = await engine.GetPortfolioAsync(UserId);

        Assert.True(result.IsSuccess);

        persistence.Verify(x => x.GetPortfolioAsync(UserId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
