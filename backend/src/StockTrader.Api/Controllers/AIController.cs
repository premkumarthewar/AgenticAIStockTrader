using Microsoft.AspNetCore.Mvc;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController(ITradingAdvisorService tradingAdvisorService) : ControllerBase
{
    [HttpGet("decision")]
    [ProducesResponseType(typeof(Result<TradingDecisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TradingDecisionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Decision([FromQuery] AnalyzeStockRequest request, CancellationToken cancellationToken)
    {
        Result<TradingDecisionDto> result = await tradingAdvisorService.AnalyzeAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("market-analysis")]
    public async Task<ActionResult<AnalyzeStockResponse>> Market([FromQuery] AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken)
    {
        Result<AnalyzeStockResponse> response = await tradingAdvisorService.AnalyzeMarketAsync(analyzeStockRequest, cancellationToken);

        return Ok(response);
    }

    [HttpGet("research")]
    public async Task<ActionResult<AnalyzeStockResponse>> Research(AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken)
    {
        Result<AnalyzeStockResponse> response = await tradingAdvisorService.ResearchAsync(analyzeStockRequest, cancellationToken);

        if (response.IsFailure)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("portfolio-analysis")]
    [ProducesResponseType(typeof(Result<PortfolioRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PortfolioRecommendationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PortfolioAnalysis([FromBody] PortfolioAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        Result<PortfolioRecommendationDto> result = await tradingAdvisorService.AnalyzePortfolioAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("watchlist-analysis")]
    public async Task<IActionResult> WatchlistAnalysis([FromBody] WatchlistDto request, CancellationToken cancellationToken)
    {
        Task<Result<WatchlistAnalysisDto>> result = tradingAdvisorService.AnalyzeWatchlistAsync(request, cancellationToken);

        if (result.IsFaulted)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("memory/{symbol}")]
    public async Task<IActionResult> GetMemory(string symbol, CancellationToken cancellationToken)
    {
        Result<MemoryResponseDto> result = await tradingAdvisorService.GetMemoryAsync(symbol, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result);

        return Ok(result);
    }
}
