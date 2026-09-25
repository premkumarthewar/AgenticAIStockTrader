using Microsoft.AspNetCore.Mvc;
using StockTrader.Application.Watchlist.Dtos;
using StockTrader.Application.Watchlist.Interfaces;
using StockTrader.Contracts.Requests;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

/// <summary>
/// CRUD for the persisted watchlist that MarketMonitoringBackgroundService watches in the background, plus the alert history it records. This is separate from AIController because it's plain data management, not an AI-driven action.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WatchlistController(IWatchlistService watchlistService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<WatchlistItemDto>> result = await watchlistService.GetAllAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddWatchlistSymbolRequest request, CancellationToken cancellationToken)
    {
        Result<WatchlistItemDto> result = await watchlistService.AddAsync(request.Symbol, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpDelete("{symbol}")]
    public async Task<IActionResult> Remove(string symbol, CancellationToken cancellationToken)
    {
        Result result = await watchlistService.RemoveAsync(symbol, cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return NoContent();
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetRecentAlerts([FromQuery] int count, CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<MonitoringAlertDto>> result = await watchlistService.GetRecentAlertsAsync(count, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
