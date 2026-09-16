using StockTrader.AI.RiskManagement.Interfaces;
using StockTrader.Application.RiskManagement.Dtos;
using StockTrader.Application.RiskManagement.Interfaces;
using StockTrader.Shared.Results;

namespace StockTrader.AI.Services;

public sealed class RiskManagementService(IRiskEngine riskEngine) : IRiskManagementService
{
    public Task<Result<RiskAssessmentDto>> AssessAsync(
        RiskRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return riskEngine.AssessAsync(
            request,
            cancellationToken);
    }
}
