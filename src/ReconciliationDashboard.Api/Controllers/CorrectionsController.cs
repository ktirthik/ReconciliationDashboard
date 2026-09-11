using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReconciliationDashboard.Api.Models.Dtos;
using ReconciliationDashboard.Api.Services;

namespace ReconciliationDashboard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts/{accountId:guid}/corrections")]
public class CorrectionsController(ICorrectionService correctionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CorrectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAccount(Guid accountId, CancellationToken ct)
    {
        var corrections = await correctionService.GetByAccountAsync(accountId, ct);
        return Ok(corrections);
    }

    [HttpPost("run")]
    [ProducesResponseType(typeof(IReadOnlyList<CorrectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Run(Guid accountId, CancellationToken ct)
    {
        try
        {
            var corrections = await correctionService.RunRulesEngineAsync(accountId, ct);
            return Ok(corrections);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(Problem(ex.Message));
        }
    }
}
