using StockTrader.Application.AI.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Agents.Interfaces;

public interface IPortfolioAgent
{
    Task<Result<PortfolioRecommendationDto>> AnalyzeAsync(PortfolioAnalysisRequestDto request, CancellationToken cancellationToken = default);
}
