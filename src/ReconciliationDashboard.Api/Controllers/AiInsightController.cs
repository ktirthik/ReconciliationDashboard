using Microsoft.AspNetCore.Mvc;
using ReconciliationDashboard.Api.Models.Dtos;
using ReconciliationDashboard.Api.Services;

namespace ReconciliationDashboard.Api.Controllers;

[ApiController]
[Route("api/accounts/{accountId:guid}/ai-insight")]
public class AiInsightController(IAiInsightService aiService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AiInsightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid accountId, CancellationToken ct)
    {
        try
        {
            var insight = await aiService.GetInsightAsync(accountId, ct);
            return Ok(insight);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
