using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Messenger.Api.Tests;

/// <summary>
/// Политика именования проверяется на настройках, которые приложение реально
/// отдает контроллерам, а не на отдельно собранных в тесте.
/// </summary>
public sealed class JsonNamingTests(MessengerApiFactory factory)
    : IClassFixture<MessengerApiFactory>
{
    private sealed record Person(string FirstName, string SecondName);

    private JsonSerializerOptions Options =>
        factory.Services
            .GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>()
            .Value.JsonSerializerOptions;

    [Fact(DisplayName = "PascalCase сериализуется в snake_case")]
    public void Serializes_to_snake_case()
    {
        var json = JsonSerializer.Serialize(new Person("Petya", "Pupkin"), Options);

        Assert.Contains("\"first_name\"", json);
        Assert.Contains("\"second_name\"", json);
        Assert.DoesNotContain("firstName", json);
    }

    [Fact(DisplayName = "snake_case разбирается обратно в PascalCase")]
    public void Deserializes_from_snake_case()
    {
        var person = JsonSerializer.Deserialize<Person>(
            """{"first_name":"Petya","second_name":"Pupkin"}""", Options);

        Assert.NotNull(person);
        Assert.Equal("Petya", person.FirstName);
        Assert.Equal("Pupkin", person.SecondName);
    }
}
