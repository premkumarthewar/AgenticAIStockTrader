using Microsoft.AspNetCore.Mvc;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Backtesting.Dtos;
using StockTrader.Application.Backtesting.Interfaces;
using StockTrader.Application.Common.Interfaces;
using StockTrader.Application.PaperTrading.Dtos;
using StockTrader.Application.PaperTrading.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Contracts.Responses;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController(ITradingAdvisorService tradingAdvisorService, IBacktestingService backtestingService, IPaperTradingService paperTradingService) : ControllerBase
{
    [HttpGet("decision")]
    [ProducesResponseType(typeof(Result<TradingDecisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<TradingDecisionDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Decision([FromQuery] AnalyzeStockRequest request, CancellationToken cancellationToken)
    {
        Result<TradingDecisionDto> result = await tradingAdvisorService.AnalyzeAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("decision/execute")]
    [ProducesResponseType(typeof(Result<ExecutionResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<ExecutionResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteDecision([FromQuery] AnalyzeStockRequest request, CancellationToken cancellationToken)
    {
        Result<ExecutionResultDto> result = await tradingAdvisorService.AnalyzeAndExecuteAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("market-analysis")]
    public async Task<ActionResult<AnalyzeStockResponse>> Market([FromQuery] AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken)
    {
        Result<AnalyzeStockResponse> response = await tradingAdvisorService.AnalyzeMarketAsync(analyzeStockRequest, cancellationToken);

        if (response.IsFailure)
            return BadRequest(response.Error);

        return Ok(response.Value);
    }

    [HttpGet("research")]
    public async Task<ActionResult<AnalyzeStockResponse>> Research([FromQuery] AnalyzeStockRequest analyzeStockRequest, CancellationToken cancellationToken)
    {
        Result<AnalyzeStockResponse> response = await tradingAdvisorService.ResearchAsync(analyzeStockRequest, cancellationToken);

        if (response.IsFailure)
            return BadRequest(response.Error);

        return Ok(response.Value);
    }

    [HttpPost("portfolio-analysis")]
    [ProducesResponseType(typeof(Result<PortfolioRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PortfolioRecommendationDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PortfolioAnalysis([FromBody] PortfolioAnalysisRequestDto request, CancellationToken cancellationToken)
    {
        Result<PortfolioRecommendationDto> result = await tradingAdvisorService.AnalyzePortfolioAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("watchlist-analysis")]
    public async Task<IActionResult> WatchlistAnalysis([FromBody] WatchlistDto request, CancellationToken cancellationToken)
    {
        Result<WatchlistAnalysisDto> result = await tradingAdvisorService.AnalyzeWatchlistAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("memory/{symbol}")]
    public async Task<IActionResult> GetMemory(string symbol, CancellationToken cancellationToken)
    {
        Result<MemoryResponseDto> result = await tradingAdvisorService.GetMemoryAsync(symbol, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }


    [HttpPost("backtest")]
    public async Task<IActionResult> Backtest([FromBody] BacktestRequestDto request, CancellationToken cancellationToken)
    {
        Result<BacktestResultDto> result = await backtestingService.RunAsync(request, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }


    [HttpPost("paper-trading/trade")]
    public async Task<IActionResult> ExecutePaperTrade(
    [FromBody] PaperTradeRequestDto request,
    CancellationToken cancellationToken)
    {
        Result<PaperPortfolioDto> result =
            await paperTradingService.ExecuteTradeAsync(
                request,
                cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("paper-trading/portfolio")]
    public async Task<IActionResult> GetPaperPortfolio(
    CancellationToken cancellationToken)
    {
        Result<PaperPortfolioDto> result =
            await paperTradingService.GetPortfolioAsync(
                cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("paper-trading/position/{symbol}")]
    public async Task<IActionResult> GetPaperPosition(
    string symbol,
    CancellationToken cancellationToken)
    {
        Result<PaperPositionDto> result =
            await paperTradingService.GetPositionAsync(
                symbol,
                cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("paper-trading/initialize")]
    public async Task<IActionResult> InitializePaperTrading(
    [FromQuery] decimal initialCapital,
    CancellationToken cancellationToken)
    {
        Result<PaperPortfolioDto> result = await paperTradingService.InitializePortfolioAsync(initialCapital, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
