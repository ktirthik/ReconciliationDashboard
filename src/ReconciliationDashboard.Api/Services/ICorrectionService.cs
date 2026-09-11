using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public interface ICorrectionService
{
    Task<IReadOnlyList<CorrectionDto>> GetByAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<CorrectionDto>> RunRulesEngineAsync(Guid accountId, CancellationToken ct = default);
    Task<CorrectionDto?> SetStatusAsync(Guid accountId, Guid correctionId, CorrectionStatus status, CancellationToken ct = default);
}
