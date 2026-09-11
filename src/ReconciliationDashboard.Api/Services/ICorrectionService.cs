using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public interface ICorrectionService
{
    Task<IReadOnlyList<CorrectionDto>> GetByAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<CorrectionDto>> RunRulesEngineAsync(Guid accountId, CancellationToken ct = default);
}
