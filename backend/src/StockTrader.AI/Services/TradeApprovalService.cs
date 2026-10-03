using StockTrader.AI.TradeExecution.Interfaces;
using StockTrader.Application.AI.Dtos;
using StockTrader.Application.Approvals.Dtos;
using StockTrader.Application.Approvals.Interfaces;
using StockTrader.Application.Broker.Dtos;
using StockTrader.Domain.Entities;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Services;

/// <summary>
/// Owns the human-approval gate that sits between an AI trading decision and a real
/// broker order. A BUY/SELL decision becomes a Pending TradeApproval here; nothing is
/// ever sent to IBrokerTradeExecutor until ApproveAsync is called for that specific
/// approval, and RejectAsync guarantees a broker call never happens for it at all.
/// </summary>
public sealed class TradeApprovalService(
    ITradeApprovalPersistence persistence,
    IBrokerTradeExecutor brokerTradeExecutor) : ITradeApprovalService
{
    public async Task<Result<TradeApprovalDto>> RequestApprovalAsync(TradingDecisionDto decision, BrokerProvider brokerProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);

        string action = decision.Decision.Trim().ToUpperInvariant();

        if (action is not ("BUY" or "SELL"))
            return Result<TradeApprovalDto>.Failure(new Error("BadRequest", $"'{decision.Decision}' is not a decision that requires broker approval."));

        decimal targetPrice = action == "BUY" ? decision.TargetBuyPrice ?? 0m : decision.TargetSellPrice ?? 0m;

        TradeApproval approval = new()
        {
            Id = Guid.NewGuid(),
            Symbol = decision.Symbol,
            Action = action,
            Quantity = decision.RecommendedQuantity,
            TargetPrice = targetPrice,
            Confidence = decision.Confidence,
            RiskLevel = decision.RiskLevel,
            Reasoning = decision.Reasoning,
            BrokerProvider = brokerProvider,
            Status = TradeApprovalStatus.Pending,
            RequestedOnUtc = DateTime.UtcNow
        };

        await persistence.AddAsync(approval, cancellationToken);

        await persistence.SaveChangesAsync(cancellationToken);

        return Result<TradeApprovalDto>.Success(Map(approval));
    }

    public async Task<Result<TradeApprovalDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TradeApproval? approval = await persistence.GetByIdAsync(id, cancellationToken);

        if (approval is null)
            return Result<TradeApprovalDto>.Failure(new Error("NotFound", $"No trade approval was found with id '{id}'."));

        return Result<TradeApprovalDto>.Success(Map(approval));
    }

    public async Task<Result<IReadOnlyList<TradeApprovalDto>>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TradeApproval> approvals = await persistence.GetPendingAsync(cancellationToken);

        return Result<IReadOnlyList<TradeApprovalDto>>.Success([.. approvals.Select(Map)]);
    }

    public async Task<Result<IReadOnlyList<TradeApprovalDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TradeApproval> approvals = await persistence.GetAllAsync(cancellationToken);

        return Result<IReadOnlyList<TradeApprovalDto>>.Success([.. approvals.Select(Map)]);
    }

    public async Task<Result<TradeApprovalDto>> ApproveAsync(Guid id, string? notes, CancellationToken cancellationToken = default)
    {
        TradeApproval? approval = await persistence.GetByIdAsync(id, cancellationToken);

        if (approval is null)
            return Result<TradeApprovalDto>.Failure(new Error("NotFound", $"No trade approval was found with id '{id}'."));

        if (approval.Status != TradeApprovalStatus.Pending)
            return Result<TradeApprovalDto>.Failure(
                new Error("Conflict", $"Trade approval '{id}' is '{approval.Status}' and can no longer be approved."));

        approval.Status = TradeApprovalStatus.Approved;
        approval.DecidedOnUtc = DateTime.UtcNow;
        approval.DecisionNotes = notes;

        // The human decision is saved before the broker call is attempted, so an approval
        // is never lost even if the broker call itself throws or times out.
        await persistence.SaveChangesAsync(cancellationToken);

        Result<BrokerOrderDto> executionResult = await brokerTradeExecutor.ExecuteApprovedTradeAsync(Map(approval), cancellationToken);

        if (executionResult.IsSuccess)
        {
            approval.Status = TradeApprovalStatus.Executed;
            approval.BrokerOrderId = executionResult.Value.BrokerOrderId;
        }
        else
        {
            approval.Status = TradeApprovalStatus.ExecutionFailed;
            approval.ExecutionError = executionResult.Error.Message;
        }

        await persistence.SaveChangesAsync(cancellationToken);

        return Result<TradeApprovalDto>.Success(Map(approval));
    }

    public async Task<Result<TradeApprovalDto>> RejectAsync(Guid id, string? notes, CancellationToken cancellationToken = default)
    {
        TradeApproval? approval = await persistence.GetByIdAsync(id, cancellationToken);

        if (approval is null)
            return Result<TradeApprovalDto>.Failure(new Error("NotFound", $"No trade approval was found with id '{id}'."));

        if (approval.Status != TradeApprovalStatus.Pending)
            return Result<TradeApprovalDto>.Failure(
                new Error("Conflict", $"Trade approval '{id}' is '{approval.Status}' and can no longer be rejected."));

        approval.Status = TradeApprovalStatus.Rejected;
        approval.DecidedOnUtc = DateTime.UtcNow;
        approval.DecisionNotes = notes;

        await persistence.SaveChangesAsync(cancellationToken);

        return Result<TradeApprovalDto>.Success(Map(approval));
    }

    private static TradeApprovalDto Map(TradeApproval approval)
    {
        return new TradeApprovalDto
        {
            Id = approval.Id,
            Symbol = approval.Symbol,
            Action = approval.Action,
            Quantity = approval.Quantity,
            TargetPrice = approval.TargetPrice,
            Confidence = approval.Confidence,
            RiskLevel = approval.RiskLevel,
            Reasoning = approval.Reasoning,
            BrokerProvider = approval.BrokerProvider.ToString(),
            Status = approval.Status.ToString(),
            RequestedOnUtc = approval.RequestedOnUtc,
            DecidedOnUtc = approval.DecidedOnUtc,
            DecisionNotes = approval.DecisionNotes,
            BrokerOrderId = approval.BrokerOrderId,
            ExecutionError = approval.ExecutionError
        };
    }
}
