namespace StockTrader.Application.AI.Dtos;

public sealed record MemoryResponseDto
{
    public IReadOnlyCollection<string> Entries { get; init; } = [];
}
