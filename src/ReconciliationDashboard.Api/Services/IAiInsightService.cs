using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public interface IAiInsightService
{
    Task<AiInsightDto> GetInsightAsync(Guid accountId, CancellationToken ct);
}
