using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Auth.IntegrationTests;

public sealed class UserCrudFlowTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task AuthenticatedUserShouldManageOwnAccountWhenPostgresIsAvailable()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")))
        {
            return;
        }

        using var client = factory.CreateClient();
        var email = $"crud-{Guid.NewGuid():N}@example.com";
        var registration = await client.PostAsJsonAsync("/auth/register", new { email, password = "SenhaForte@2026" });
        var registrationJson = JsonDocument.Parse(await registration.Content.ReadAsStringAsync());
        var userId = registrationJson.RootElement.GetProperty("id").GetGuid();

        var login = await client.PostAsJsonAsync("/auth/login", new { email, password = "SenhaForte@2026" });
        var loginJson = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.RootElement.GetProperty("accessToken").GetString());

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/users/{userId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/users?page=1&pageSize=10")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/users/{userId}", new { password = "OutraSenha@2026" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/users/{userId}")).StatusCode);
    }
}
