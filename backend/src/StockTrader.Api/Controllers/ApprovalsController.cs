using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTrader.Application.Approvals.Dtos;
using StockTrader.Application.Approvals.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.Api.Controllers;

/// <summary>
/// The human-approval gate on broker-bound trades. Separate from AIController because
/// this is the human-in-the-loop workflow, not an AI-driven action: nothing here calls
/// the AI, and Approve is the only place in the API that can trigger a real broker order.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApprovalsController(ITradeApprovalService tradeApprovalService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<TradeApprovalDto>> result = await tradeApprovalService.GetAllAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending(CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<TradeApprovalDto>> result = await tradeApprovalService.GetPendingAsync(cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        Result<TradeApprovalDto> result = await tradeApprovalService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Approves the trade and immediately attempts to place it with the broker. The
    /// response's Status distinguishes a successfully placed order (Executed, with
    /// BrokerOrderId set) from one the broker itself rejected after approval
    /// (ExecutionFailed, with ExecutionError set).
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApprovalDecisionRequestDto? request, CancellationToken cancellationToken)
    {
        Result<TradeApprovalDto> result = await tradeApprovalService.ApproveAsync(id, request?.Notes, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ApprovalDecisionRequestDto? request, CancellationToken cancellationToken)
    {
        Result<TradeApprovalDto> result = await tradeApprovalService.RejectAsync(id, request?.Notes, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
