namespace StockTrader.Application.AI.Dtos;

public sealed record PortfolioAnalysisRequestDto
{
    public IReadOnlyList<PortfolioHoldingDto> Holdings { get; init; }
        = [];

    public decimal AvailableCash { get; init; }
}
