namespace StockTrader.Application.PaperTrading.Dtos;

public sealed record PaperPortfolioDto
{
    public decimal InitialCapital { get; init; }

    public decimal CashBalance { get; init; }

    public decimal InvestedValue { get; init; }

    public decimal CurrentPortfolioValue { get; init; }

    public decimal ProfitLoss { get; init; }

    public decimal ReturnPercentage { get; init; }

    public IReadOnlyList<PaperPositionDto> Positions { get; init; }
        = [];

    public IReadOnlyList<PaperTradeDto> Trades { get; init; }
        = [];
}