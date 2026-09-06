using Microsoft.AspNetCore.Diagnostics;

namespace Messenger.Api.Errors;

/// <summary>
/// Последний рубеж: необработанное исключение логируется целиком, а наружу
/// уходит <c>500</c> с обезличенным текстом - ни стека, ни SQL, ни имен таблиц.
/// </summary>
public sealed class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger)
    : IExceptionHandler
{
    private const string PublicReason = "Internal server error";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "Необработанная ошибка при обработке {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(PublicReason),
            cancellationToken);

        return true;
    }
}
