namespace Messenger.Api.Configuration;

/// <summary>
/// Строка подключения к PostgreSQL. Читается на старте, чтобы приложение
/// падало сразу и внятно, а не на первом запросе к БД.
/// </summary>
public static class DatabaseConfiguration
{
    public const string ConnectionName = "Postgres";

    /// <summary>
    /// Возвращает строку подключения или бросает исключение с подсказкой.
    /// Формат сменился вместе со стеком: Npgsql ждет
    /// <c>Host=...;Username=...</c>, а не URI <c>postgres://...</c>, - старое
    /// значение в <c>.env</c> молча не подойдет.
    /// </summary>
    public static string GetPostgresConnectionString(this IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionName);

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        throw new InvalidOperationException(
            $"Не задана строка подключения ConnectionStrings:{ConnectionName}. " +
            "Задайте переменную окружения ConnectionStrings__Postgres в формате Npgsql, " +
            "например Host=localhost;Port=5432;Database=messenger;Username=messenger;Password=messenger " +
            "(URI postgres://... провайдер не понимает). Образец - в .env.example.");
    }
}
