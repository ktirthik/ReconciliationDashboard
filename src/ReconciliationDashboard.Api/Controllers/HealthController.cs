using Microsoft.AspNetCore.Mvc;

namespace ReconciliationDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new HealthResponse(
            Status: "Healthy",
            Timestamp: DateTimeOffset.UtcNow,
            Version: "1.0.0"
        ));
    }
}

public record HealthResponse(string Status, DateTimeOffset Timestamp, string Version);
