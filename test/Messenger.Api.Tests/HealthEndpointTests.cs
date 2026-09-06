using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Messenger.Api.Tests;

public sealed class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact(DisplayName = "GET /api/v2/health отвечает 200 и телом снятого Nest-эндпоинта")]
    public async Task Health_returns_the_contract_body()
    {
        var response = await factory.CreateClient().GetAsync("/api/v2/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var fields = body.EnumerateObject().Select(field => field.Name).Order();

        Assert.Equal(["status", "uptime"], fields);
        Assert.Equal("ok", body.GetProperty("status").GetString());
        Assert.True(body.GetProperty("uptime").GetInt64() >= 0);
    }

    [Fact(DisplayName = "health недоступен без префикса api/v2")]
    public async Task Health_is_not_reachable_without_the_prefix()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
