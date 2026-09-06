using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Messenger.Api.Tests;

/// <summary>
/// Приложение под тесты. Строку подключения задаем явно, а не полагаемся на
/// окружение Development: приложение падает на старте без нее, а тестам нужна
/// предсказуемость независимо от того, какой ASPNETCORE_ENVIRONMENT снаружи.
/// </summary>
public sealed class MessengerApiFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        builder.ConfigureHostConfiguration(configuration =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] =
                    "Host=localhost;Port=5432;Database=messenger_tests;Username=messenger;Password=messenger",
            }));

        // Контроллеры, которые ломаются намеренно, живут в тестовой сборке:
        // в поставляемом API им не место.
        builder.ConfigureServices(services =>
            services.AddControllers()
                .PartManager.ApplicationParts.Add(
                    new AssemblyPart(typeof(MessengerApiFactory).Assembly)));

        return base.CreateHost(builder);
    }
}
