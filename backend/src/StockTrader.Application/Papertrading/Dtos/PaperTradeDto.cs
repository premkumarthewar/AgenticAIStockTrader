namespace StockTrader.Application.PaperTrading.Dtos;

public sealed record PaperTradeDto
{
    public Guid Id { get; init; }

    public required string Symbol { get; init; }

    public required string Action { get; init; }

    public decimal Quantity { get; init; }

    public decimal Price { get; init; }

    public decimal TotalValue { get; init; }

    public DateTime ExecutedAtUtc { get; init; }
}