using Microsoft.AspNetCore.Mvc;
using ReconciliationDashboard.Api.Services;

namespace ReconciliationDashboard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    public sealed record LoginRequest(string Username, string Password);
    public sealed record LoginResponse(string Token);

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
    {
        var token = _auth.Login(req.Username, req.Password);
        if (token is null) return Unauthorized(new { message = "Invalid credentials." });
        return Ok(new LoginResponse(token));
    }
}
