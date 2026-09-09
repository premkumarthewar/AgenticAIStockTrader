namespace StockTrader.Application.AI.Dtos;

public sealed record AlertDto
{
    public required string Symbol { get; init; }

    public required string AlertType { get; init; }

    public required string Message { get; init; }

    public DateTime TriggeredAt { get; init; }
}
