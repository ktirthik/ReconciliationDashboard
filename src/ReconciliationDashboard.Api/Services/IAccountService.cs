using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Api.Services;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAllAsync(AccountStatus? status, CancellationToken ct);
    Task<AccountDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<AccountDto> CreateAsync(CreateAccountRequest request, CancellationToken ct);
    Task<AccountDto?> UpdateAsync(Guid id, UpdateAccountRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}
