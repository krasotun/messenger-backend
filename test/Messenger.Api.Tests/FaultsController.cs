using System.ComponentModel.DataAnnotations;
using Messenger.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Messenger.Api.Tests;

/// <summary>
/// Эндпоинты, ломающиеся по требованию: нужны, чтобы проверить формат ошибок.
/// Живут только в тестовой сборке и подключаются через ApplicationPart.
/// </summary>
public sealed class FaultsController : ApiControllerBase
{
    public sealed record ValidatedRequest([Required] string? FirstName);

    [HttpPost("validated")]
    public IActionResult Validated([FromBody] ValidatedRequest request) => Ok(request);

    [HttpGet("boom")]
    public IActionResult Boom() => throw new InvalidOperationException(
        "секретная внутренность: SELECT * FROM users");
}
