using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Messenger.Api.Tests;

/// <summary>
/// Контракт требует тела { "reason": ... } и ничего больше - ни type, ни
/// title, ни status из ProblemDetails.
/// </summary>
public sealed class ErrorFormatTests(MessengerApiFactory factory)
    : IClassFixture<MessengerApiFactory>
{
    private static readonly string[] ProblemDetailsFields = ["type", "title", "status", "traceId", "errors"];

    private static void AssertReasonOnly(JsonElement body)
    {
        var fields = body.EnumerateObject().Select(field => field.Name).ToArray();

        Assert.Equal(["reason"], fields);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("reason").GetString()));

        foreach (var field in ProblemDetailsFields)
        {
            Assert.DoesNotContain(field, fields);
        }
    }

    [Fact(DisplayName = "Ошибка валидации отдает 400 и только reason")]
    public async Task Validation_error_returns_reason_only()
    {
        var response = await factory.CreateClient().PostAsync(
            "/api/v2/faults/validated",
            new StringContent("{}", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertReasonOnly(await response.Content.ReadFromJsonAsync<JsonElement>());
    }

    [Fact(DisplayName = "Необработанная ошибка отдает 500 без внутренностей")]
    public async Task Unhandled_error_hides_internals()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/api/v2/faults/boom");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var raw = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SELECT", raw);
        Assert.DoesNotContain("InvalidOperationException", raw);

        AssertReasonOnly(JsonSerializer.Deserialize<JsonElement>(raw));
    }

    [Fact(DisplayName = "Неизвестный путь отдает 404 и только reason")]
    public async Task Not_found_returns_reason_only()
    {
        var response = await factory.CreateClient().GetAsync("/api/v2/nothing-here");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertReasonOnly(await response.Content.ReadFromJsonAsync<JsonElement>());
    }
}
