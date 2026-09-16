using StockTrader.AI.RiskManagement.Interfaces;
using StockTrader.Application.RiskManagement.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.RiskManagement;

public sealed class RiskEngine : IRiskEngine
{
    private const decimal MaxPositionSizePercent = 10m;
    private const decimal MaxPortfolioExposurePercent = 25m;

    public Task<Result<RiskAssessmentDto>> AssessAsync(RiskRequestDto request, CancellationToken cancellationToken = default)
    {
        decimal maxCapitalAllowed = request.PortfolioValue * MaxPositionSizePercent / 100m;

        decimal allocation = Math.Min(request.CashBalance, maxCapitalAllowed);

        decimal quantity = request.CurrentPrice == 0 ? 0 : Math.Floor(allocation / request.CurrentPrice);

        decimal exposurePercent = request.PortfolioValue == 0 ? 0 : request.ExistingExposure / request.PortfolioValue * 100m;

        bool approved = exposurePercent < MaxPortfolioExposurePercent;

        RiskAssessmentDto dto = new()
        {
            IsApproved = approved,
            Reason = approved ? "Trade approved." : "Portfolio exposure limit exceeded.",
            RecommendedQuantity = quantity,
            RecommendedCapitalAllocation = allocation,
            PortfolioExposurePercentage = exposurePercent,
            RiskScore = approved ? 25 : 90
        };

        return Task.FromResult(Result<RiskAssessmentDto>.Success(dto));
    }
}
