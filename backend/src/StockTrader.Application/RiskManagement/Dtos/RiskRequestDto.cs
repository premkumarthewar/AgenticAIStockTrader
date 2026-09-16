namespace StockTrader.Application.RiskManagement.Dtos;

public sealed record RiskRequestDto
{
    public required string Symbol { get; init; }

    public decimal CurrentPrice { get; init; }

    public decimal CashBalance { get; init; }

    public decimal PortfolioValue { get; init; }

    public decimal ExistingExposure { get; init; }
}