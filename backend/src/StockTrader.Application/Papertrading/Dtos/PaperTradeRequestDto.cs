namespace StockTrader.Application.PaperTrading.Dtos;

public sealed record PaperTradeRequestDto
{
    public required string Symbol { get; init; }

    public required string Action { get; init; }

    public int Quantity { get; init; }

    public decimal Price { get; init; }
}