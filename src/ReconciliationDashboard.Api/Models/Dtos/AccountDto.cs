namespace ReconciliationDashboard.Api.Models.Dtos;

public record AccountDto(
    Guid Id,
    string AccountNumber,
    string CustomerName,
    string Status,
    string? FlagReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);
