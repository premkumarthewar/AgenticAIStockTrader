using StockTrader.Application.RiskManagement.Dtos;
using StockTrader.Shared.Results;

namespace StockTrader.AI.RiskManagement.Interfaces;

public interface IRiskEngine
{
    Task<Result<RiskAssessmentDto>> AssessAsync(
        RiskRequestDto request,
        CancellationToken cancellationToken = default);
}
