namespace StockTrader.Application.Approvals.Dtos;

/// <summary>
/// Body of an approve/reject call against a pending TradeApproval. Notes is optional in
/// both cases (e.g. "position already covered elsewhere" on a rejection).
/// </summary>
public sealed record ApprovalDecisionRequestDto
{
    public string? Notes { get; init; }
}
