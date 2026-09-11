using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReconciliationDashboard.Api.Data;
using ReconciliationDashboard.Api.Models;
using ReconciliationDashboard.Api.Models.Dtos;

namespace ReconciliationDashboard.Tests.Controllers;

// A single shared factory replaces SQL Server with a named in-memory DB.
// All tests in this class share one factory instance (and one DB), so
// InitializeAsync re-seeds the known account before every test to handle
// the case where a delete test removed it.
public class AccountsTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove DbContextOptions<AppDbContext> AND the internal
            // IDbContextOptionsConfiguration<AppDbContext> that carries the
            // SQL Server wiring — leaving both in place causes a "two
            // providers registered" error.
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

            // Fixed name so every test in this class shares the same store
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseInMemoryDatabase("AccountsControllerTests"));
        });
    }
}

public class AccountsControllerTests(AccountsTestFactory factory)
    : IClassFixture<AccountsTestFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    private static readonly Guid KnownId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    public async ValueTask InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Re-add the known account if a delete test removed it
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
