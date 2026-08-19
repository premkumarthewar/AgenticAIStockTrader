using Microsoft.AspNetCore.Mvc;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

[ApiController]
[Route("api/ai")]
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
    public async Task<ActionResult<AnalyzeStockResponse>> Market([FromQuery]AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken)
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
}
