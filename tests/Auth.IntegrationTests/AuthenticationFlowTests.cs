using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Auth.IntegrationTests;

public sealed class AuthenticationFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public AuthenticationFlowTests(WebApplicationFactory<Program> factory) => this.factory = factory;

    [Fact]
    public async Task RegisterAndLoginShouldSucceedWhenPostgresIsAvailable()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")))
        {
            return;
        }

        using var client = factory.CreateClient();
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var registration = await client.PostAsJsonAsync("/auth/register", new { email, password = "SenhaForte@2026" });
        Assert.Equal(HttpStatusCode.Created, registration.StatusCode);

        var login = await client.PostAsJsonAsync("/auth/login", new { email, password = "SenhaForte@2026" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("accessToken", await login.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task LoginShouldRejectInvalidCredentialsWhenPostgresIsAvailable()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")))
        {
            return;
        }

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/login", new { email = "missing@example.com", password = "SenhaForte@2026" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
