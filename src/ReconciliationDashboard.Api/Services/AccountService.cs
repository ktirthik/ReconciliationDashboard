using Microsoft.EntityFrameworkCore;
using ReconciliationDashboard.Api.Data;
using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public class AccountService(AppDbContext db) : IAccountService
{
    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(AccountStatus? status, CancellationToken ct)
    {
        var query = db.Accounts.AsQueryable();
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderBy(a => a.AccountNumber)
            .Select(a => ToDto(a))
            .ToListAsync(ct);
    }

    public async Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var account = await db.Accounts.FindAsync([id], ct);
        return account is null ? null : ToDto(account);
    }

    public async Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = request.AccountNumber,
            CustomerName = request.CustomerName,
            Status = AccountStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        return ToDto(account);
    }

    public async Task<AccountDto?> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct)
    {
        var account = await db.Accounts.FindAsync([id], ct);
        if (account is null) return null;

        if (!Enum.TryParse<AccountStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            throw new ArgumentException($"'{request.Status}' is not a valid account status.");

        account.CustomerName = request.CustomerName;
        account.Status = parsedStatus;
        account.FlagReason = request.FlagReason;
        account.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return ToDto(account);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var account = await db.Accounts.FindAsync([id], ct);
        if (account is null) return false;

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static AccountDto ToDto(Account a) => new(
        a.Id,
        a.AccountNumber,
        a.CustomerName,
        a.Status.ToString(),
        a.FlagReason,
        a.CreatedAt,
        a.UpdatedAt
    );
}
