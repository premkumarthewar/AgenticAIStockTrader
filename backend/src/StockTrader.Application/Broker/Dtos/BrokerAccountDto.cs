namespace StockTrader.Application.Broker.Dtos;

public sealed record BrokerAccountDto
{
    public required string AccountId { get; init; }

    public required decimal CashBalance { get; init; }

    public required decimal PortfolioValue { get; init; }

    public required decimal BuyingPower { get; init; }

    public required bool IsPaperTrading { get; init; }

    public bool TradingBlocked { get; init; }
}
