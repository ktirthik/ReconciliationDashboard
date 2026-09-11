using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ReconciliationDashboard.Api.Services;

public interface IAuthService
{
    string? Login(string username, string password);
}

public sealed class AuthService : IAuthService
{
    // Single demo user — real auth would query a users table with hashed passwords.
    private static readonly (string Username, string Password, string Role)[] Users =
    [
        ("admin", "Admin1234!", "Admin"),
        ("analyst", "Analyst1234!", "Analyst")
    ];

    private readonly IConfiguration _config;

    public AuthService(IConfiguration config) => _config = config;

    public string? Login(string username, string password)
    {
        var user = Users.FirstOrDefault(u =>
            string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase) &&
            u.Password == password);

        if (user == default) return null;

        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
