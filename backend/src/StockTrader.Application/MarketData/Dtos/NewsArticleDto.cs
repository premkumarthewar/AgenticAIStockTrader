namespace StockTrader.Application.MarketData.Dtos;

public sealed record NewsArticleDto
{
    public required string Headline { get; init; }

    public string? Summary { get; init; }

    public string? Source { get; init; }

    public string? Url { get; init; }

    public DateTime PublishedAt { get; init; }

    public string? Category { get; init; }

    public string? RelatedSymbol { get; init; }
}
