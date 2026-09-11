using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ReconciliationDashboard.Api.Data;
using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Tests.Controllers;

// A single shared factory replaces SQL Server with an in-memory DB and
// overrides JWT settings so tests can generate valid tokens without Azure secrets.
public class AccountsTestFactory : WebApplicationFactory<Program>
{
    // Test-only JWT key — never used outside of this test assembly.
    internal const string TestJwtKey = "test-secret-key-for-unit-tests-32chars!";
    internal const string TestIssuer = "ReconciliationDashboard";
    internal const string TestAudience = "ReconciliationDashboardClient";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GenericTypeArguments.Length == 1 &&
                     d.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext) &&
                     d.ServiceType.GetGenericTypeDefinition().Name
                         .StartsWith("IDbContextOptionsConfiguration")))
                .ToList();
            foreach (var d in toRemove) services.Remove(d);

            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("AccountsControllerTests"));
        });

        // Override JWT configuration with test values
        builder.UseSetting("Jwt:Key", TestJwtKey);
        builder.UseSetting("Jwt:Issuer", TestIssuer);
        builder.UseSetting("Jwt:Audience", TestAudience);
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateToken());
        return client;
    }

    private static string GenerateToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: [new Claim(ClaimTypes.Name, "testuser"), new Claim(ClaimTypes.Role, "Admin")],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class AccountsControllerTests(AccountsTestFactory factory)
    : IClassFixture<AccountsTestFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateAuthenticatedClient();

    private static readonly Guid KnownId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    public async ValueTask InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!await db.Accounts.AnyAsync(a => a.Id == KnownId))
        {
            var now = DateTimeOffset.UtcNow;
            db.Accounts.Add(new Account
            {
                Id = KnownId,
                AccountNumber = "TEST-001",
                CustomerName = "Test Customer",
                Status = AccountStatus.Active,
                CreatedAt = now,
                UpdatedAt = now
            });
            await db.SaveChangesAsync();
        }
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // --- GET /api/accounts ---

    [Fact]
    public async Task GetAll_ReturnsOkWithAccounts()
    {
        var response = await _client.GetAsync("/api/accounts", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var accounts = await response.Content
            .ReadFromJsonAsync<List<AccountDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(accounts);
        Assert.NotEmpty(accounts);
    }

    [Fact]
    public async Task GetAll_FilterByStatus_ReturnsOnlyMatchingAccounts()
    {
        var response = await _client.GetAsync("/api/accounts?status=Active", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var accounts = await response.Content
            .ReadFromJsonAsync<List<AccountDto>>(TestContext.Current.CancellationToken);
        Assert.NotNull(accounts);
        Assert.All(accounts, a => Assert.Equal("Active", a.Status));
    }

    [Fact]
    public async Task GetAll_InvalidStatus_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/api/accounts?status=NotAStatus", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Unauthenticated_ReturnsUnauthorized()
    {
        var anonClient = factory.CreateClient();
        var response = await anonClient.GetAsync("/api/accounts", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- GET /api/accounts/{id} ---

    [Fact]
    public async Task GetById_KnownId_ReturnsAccount()
    {
        var response = await _client.GetAsync($"/api/accounts/{KnownId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var account = await response.Content
            .ReadFromJsonAsync<AccountDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(account);
        Assert.Equal("TEST-001", account.AccountNumber);
    }

    [Fact]
    public async Task GetById_UnknownId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/accounts/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- POST /api/accounts ---

    [Fact]
    public async Task Create_ValidRequest_Returns201WithLocation()
    {
        var request = new CreateAccountRequest("ACC-NEW-" + Guid.NewGuid().ToString()[..8], "New Customer");
        var response = await _client.PostAsJsonAsync("/api/accounts", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content
            .ReadFromJsonAsync<AccountDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(created);
        Assert.Equal("Active", created.Status);
    }

    [Fact]
    public async Task Create_MissingAccountNumber_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/accounts",
            new { CustomerName = "No Number" },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- PUT /api/accounts/{id} ---

    [Fact]
    public async Task Update_KnownId_ReturnsUpdatedAccount()
    {
        var request = new UpdateAccountRequest("Updated Name", "Flagged", "Test flag reason");
        var response = await _client.PutAsJsonAsync($"/api/accounts/{KnownId}", request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content
            .ReadFromJsonAsync<AccountDto>(TestContext.Current.CancellationToken);
        Assert.NotNull(updated);
        Assert.Equal("Updated Name", updated.CustomerName);
        Assert.Equal("Flagged", updated.Status);
    }

    [Fact]
    public async Task Update_InvalidStatus_ReturnsBadRequest()
    {
        var request = new UpdateAccountRequest("X", "NotAValidStatus", null);
        var response = await _client.PutAsJsonAsync($"/api/accounts/{KnownId}", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_UnknownId_ReturnsNotFound()
    {
        var request = new UpdateAccountRequest("X", "Active", null);
        var response = await _client.PutAsJsonAsync($"/api/accounts/{Guid.NewGuid()}", request, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- DELETE /api/accounts/{id} ---

    [Fact]
    public async Task Delete_KnownId_Returns204()
    {
        var response = await _client.DeleteAsync($"/api/accounts/{KnownId}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_UnknownId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/accounts/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
