using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReconciliationDashboard.Api.Controllers;
using ReconciliationDashboard.Api.Data;

namespace ReconciliationDashboard.Tests.Controllers;

// Replaces SQL Server with InMemory so health tests run without a real database.
public class HealthTestFactory : WebApplicationFactory<Program>
{
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
                opts.UseInMemoryDatabase("HealthControllerTests"));
        });

        builder.UseSetting("Jwt:Key", AccountsTestFactory.TestJwtKey);
        builder.UseSetting("Jwt:Issuer", AccountsTestFactory.TestIssuer);
        builder.UseSetting("Jwt:Audience", AccountsTestFactory.TestAudience);
    }
}

public class HealthControllerTests(HealthTestFactory factory)
    : IClassFixture<HealthTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthyStatus()
    {
        var response = await _client.GetAsync("/api/health", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
        Assert.Equal("1.0.0", body.Version);
    }
}
