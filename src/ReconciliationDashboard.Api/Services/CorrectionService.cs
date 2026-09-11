using Microsoft.EntityFrameworkCore;
using ReconciliationDashboard.Api.Data;
using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public class CorrectionService(AppDbContext db) : ICorrectionService
{
    public async Task<IReadOnlyList<CorrectionDto>> GetByAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        var records = await db.Corrections
            .Where(c => c.AccountId == accountId)
            .ToListAsync(ct);

        return records.OrderByDescending(c => c.CreatedAt).Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<CorrectionDto>> RunRulesEngineAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await db.Accounts.FindAsync([accountId], ct)
            ?? throw new KeyNotFoundException($"Account {accountId} not found.");

        var generated = ApplyRules(account);

        // Avoid duplicating correction types already pending for this account
        var existingTypes = await db.Corrections
            .Where(c => c.AccountId == accountId && c.Status == CorrectionStatus.Pending)
            .Select(c => c.CorrectionType)
            .ToListAsync(ct);

        var newRecords = generated
            .Where(g => !existingTypes.Contains(g.CorrectionType))
            .ToList();

        if (newRecords.Count > 0)
        {
            db.Corrections.AddRange(newRecords);
            await db.SaveChangesAsync(ct);
        }

        return newRecords.Select(ToDto).ToList();
    }

    private static List<CorrectionRecord> ApplyRules(Account account)
    {
        var now = DateTimeOffset.UtcNow;
        var flag = account.FlagReason?.ToLowerInvariant() ?? string.Empty;
        var results = new List<CorrectionRecord>();

        if (flag.Contains("interest suppression") || flag.Contains("interest"))
        {
            results.Add(new CorrectionRecord
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CorrectionType = CorrectionType.InterestSuppression,
                Description = "Remove incorrectly applied interest suppression and recalculate charges at standard rate.",
                OriginalValue = "Interest suppressed (£0.00 accrued)",
                CorrectedValue = "Standard interest reinstated per account terms",
                Status = CorrectionStatus.Pending,
                CreatedAt = now
            });
        }

        if (flag.Contains("meter") || flag.Contains("supply point") || flag.Contains("billing unit"))
        {
            results.Add(new CorrectionRecord
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CorrectionType = CorrectionType.MeterRemap,
                Description = "Remap billing unit to correct active supply point and retire decommissioned meter reference.",
                OriginalValue = "Meter linked to decommissioned supply point",
                CorrectedValue = "Meter remapped to current active supply point",
                Status = CorrectionStatus.Pending,
                CreatedAt = now
            });
        }

        if (flag.Contains("classification") || flag.Contains("product category") || flag.Contains("category"))
        {
            results.Add(new CorrectionRecord
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CorrectionType = CorrectionType.ClassificationError,
                Description = "Reclassify item under the correct product category to restore downstream billing logic.",
                OriginalValue = "Item registered under incorrect product category",
                CorrectedValue = "Item reclassified to correct department/category",
                Status = CorrectionStatus.Pending,
                CreatedAt = now
            });
        }

        if (flag.Contains("balance") || flag.Contains("direct debit") || flag.Contains("90-day") || flag.Contains("threshold"))
        {
            results.Add(new CorrectionRecord
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CorrectionType = CorrectionType.DebtRestructure,
                Description = "Initiate debt restructure: cancel failed direct debit mandates and set up a revised repayment schedule.",
                OriginalValue = "Outstanding balance exceeds 90-day threshold; two failed direct debits",
                CorrectedValue = "Revised repayment plan applied; new direct debit mandate issued",
                Status = CorrectionStatus.Pending,
                CreatedAt = now
            });
        }

        if (results.Count == 0)
        {
            results.Add(new CorrectionRecord
            {
                Id = Guid.NewGuid(),
                AccountId = account.Id,
                CorrectionType = CorrectionType.ManualReview,
                Description = "No automatic rule matched. Account queued for manual review by a billing analyst.",
                OriginalValue = account.FlagReason,
                CorrectedValue = null,
                Status = CorrectionStatus.Pending,
                CreatedAt = now
            });
        }

        return results;
    }

    private static CorrectionDto ToDto(CorrectionRecord r) => new(
        r.Id, r.AccountId,
        r.CorrectionType.ToString(),
        r.Description,
        r.OriginalValue,
        r.CorrectedValue,
        r.Status.ToString(),
        r.CreatedAt,
        r.AppliedAt
    );
}
