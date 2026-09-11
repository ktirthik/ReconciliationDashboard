namespace ReconciliationDashboard.Api.Models;

public class CorrectionRecord
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public CorrectionType CorrectionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }
    public string? CorrectedValue { get; set; }
    public CorrectionStatus Status { get; set; } = CorrectionStatus.Pending;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AppliedAt { get; set; }
}

public enum CorrectionType
{
    InterestSuppression,
    MeterRemap,
    ClassificationError,
    DebtRestructure,
    ManualReview
}

public enum CorrectionStatus
{
    Pending,
    Applied,
    Rejected
}
