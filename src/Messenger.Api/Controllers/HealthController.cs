using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Api.Controllers;

/// <summary>
/// Ответ health-эндпоинта. Контрактом не описан: это внутренняя проверка живости.
/// </summary>
/// <param name="Status">Всегда <c>ok</c>: сам факт ответа и есть проверка.</param>
/// <param name="Uptime">Время работы процесса, целое число секунд.</param>
public sealed record HealthResponse(string Status, long Uptime);

public sealed class HealthController : ApiControllerBase
{
    // Время старта процесса, а не машины: повторяет process.uptime() снятого Nest-эндпоинта.
    private static readonly DateTime ProcessStartedAtUtc =
        Process.GetCurrentProcess().StartTime.ToUniversalTime();

    [HttpGet]
    public HealthResponse Check() =>
        new("ok", (long)(DateTime.UtcNow - ProcessStartedAtUtc).TotalSeconds);
}
