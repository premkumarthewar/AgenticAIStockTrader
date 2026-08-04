namespace StockTrader.Application.MarketData.Dtos;

public class CompanyProfileDto
{
    public string Symbol { get; init; } = "";

    public string CompanyName { get; init; } = "";

    public string Country { get; init; } = "";

    public string Currency { get; init; } = "";

    public string Exchange { get; init; } = "";

    public string Industry { get; init; } = "";

    public string Website { get; init; } = "";

    public decimal MarketCapitalization { get; init; }

    public string Description { get; init; } = "";
}
