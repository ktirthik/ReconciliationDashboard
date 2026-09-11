namespace ReconciliationDashboard.Api.Events;

public sealed record AccountFlaggedEvent(
    Guid AccountId,
    string AccountNumber,
    string CustomerName,
    string Status,
    string? FlagReason,
    DateTime OccurredAt);
