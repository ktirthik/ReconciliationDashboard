namespace ReconciliationDashboard.Api.Models.Dtos;

public record CorrectionDto(
    Guid Id,
    Guid AccountId,
    string CorrectionType,
    string Description,
    string? OriginalValue,
    string? CorrectedValue,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? AppliedAt
);
