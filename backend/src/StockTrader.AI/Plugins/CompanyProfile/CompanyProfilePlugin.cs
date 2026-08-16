using Microsoft.SemanticKernel;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.MarketData.Dtos;
using StockTrader.Shared.Results;
using System.ComponentModel;
using System.Text.Json;

namespace StockTrader.AI.Plugins.CompanyProfile;

/// <summary>
/// Provides company profile information to AI agents, including industry, market capitalization, and other relevant details for a given stock symbol.
/// </summary>
/// <param name="service"></param>
public sealed class CompanyProfilePlugin(IStockMarketService service)
{
    /// <summary>
    /// Gets basic company information for a stock symbol.
    /// </summary>
    /// <param name="symbol">The stock ticker symbol.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A JSON string representing the company profile or an error message.</returns>
    [KernelFunction("get_company_profile")]
    [Description("Gets the company profile, industry, market capitalization, and other relevant information for a given stock symbol.")]
    public async Task<string> GetCompanyProfileAsync([Description("The stock ticker symbol, for example AAPL or MSFT.")] string symbol, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(symbol, nameof(symbol));

        Result<CompanyProfileDto> result = await service.GetCompanyProfileAsync(symbol, cancellationToken);

        if (result.IsFailure)
            return $"Unable to retrieve company profile for symbol '{symbol}': {result.Error}";

        return JsonSerializer.Serialize(result.Value);
    }
}
