using Microsoft.AspNetCore.Mvc;
using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;
using ReconciliationDashboard.Api.Services;

namespace ReconciliationDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IAccountService accountService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        CancellationToken ct)
    {
        AccountStatus? parsedStatus = null;
        if (status is not null)
        {
            if (!Enum.TryParse<AccountStatus>(status, ignoreCase: true, out var s))
                return BadRequest(Problem($"'{status}' is not a valid status. Valid values: {string.Join(", ", Enum.GetNames<AccountStatus>())}"));
            parsedStatus = s;
        }

        var accounts = await accountService.GetAllAsync(parsedStatus, ct);
        return Ok(accounts);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var account = await accountService.GetByIdAsync(id, ct);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var account = await accountService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
    {
        try
        {
            var account = await accountService.UpdateAsync(id, request, ct);
            return account is null ? NotFound() : Ok(account);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(Problem(ex.Message));
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await accountService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
