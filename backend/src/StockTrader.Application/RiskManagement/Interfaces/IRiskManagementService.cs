using StockTrader.Application.RiskManagement.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.Application.RiskManagement.Interfaces;

public interface IRiskManagementService
{
    Task<Result<RiskAssessmentDto>> AssessAsync(
        RiskRequestDto request,
        CancellationToken cancellationToken = default);
}
