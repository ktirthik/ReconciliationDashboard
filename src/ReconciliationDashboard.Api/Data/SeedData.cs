using ReconciliationDashboard.Api.Models;

namespace ReconciliationDashboard.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Accounts.Any()) return;

        var now = DateTimeOffset.UtcNow;
        var accounts = new List<Account>
        {
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0001",
                CustomerName = "Hargreaves Industrial Ltd",
                Status = AccountStatus.Active,
                CreatedAt = now.AddDays(-120),
                UpdatedAt = now.AddDays(-5)
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0002",
                CustomerName = "Patel & Sons Retail",
                Status = AccountStatus.Flagged,
                FlagReason = "Meter reference mismatch — billing unit linked to decommissioned supply point",
                CreatedAt = now.AddDays(-90),
                UpdatedAt = now.AddDays(-2)
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0003",
                CustomerName = "Meridian Property Group",
                Status = AccountStatus.AtRisk,
                FlagReason = "Outstanding balance exceeds 90-day threshold; two failed direct debit attempts",
                CreatedAt = now.AddDays(-200),
                UpdatedAt = now.AddDays(-1)
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0004",
                CustomerName = "Calloway Logistics",
                Status = AccountStatus.Active,
                CreatedAt = now.AddDays(-60),
                UpdatedAt = now.AddDays(-60)
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0005",
                CustomerName = "Thornton Care Homes",
                Status = AccountStatus.Flagged,
                FlagReason = "Interest suppression rule applied in error — correction pending review",
                CreatedAt = now.AddDays(-45),
                UpdatedAt = now
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0006",
                CustomerName = "Greenfield Solar Co",
                Status = AccountStatus.Active,
                CreatedAt = now.AddDays(-15),
                UpdatedAt = now.AddDays(-15)
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0007",
                CustomerName = "Drummond & Associates",
                Status = AccountStatus.Closed,
                CreatedAt = now.AddDays(-365),
                UpdatedAt = now.AddDays(-30)
            },
            new() {
                Id = Guid.NewGuid(),
                AccountNumber = "ACC-0008",
                CustomerName = "Rydell Manufacturing",
                Status = AccountStatus.AtRisk,
                FlagReason = "Classification dispute — item registered under incorrect product category causing downstream billing error",
                CreatedAt = now.AddDays(-180),
                UpdatedAt = now.AddDays(-3)
            },
        };

        db.Accounts.AddRange(accounts);
        await db.SaveChangesAsync();
    }
}
