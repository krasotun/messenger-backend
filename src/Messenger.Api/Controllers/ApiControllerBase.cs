using Microsoft.AspNetCore.Mvc;

namespace Messenger.Api.Controllers;

/// <summary>
/// Базовый контроллер API. Префикс <c>api/v2</c> задается здесь один раз и
/// повторяет форму контракта: <c>auth</c>, <c>user</c>, <c>chats</c>.
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
public abstract class ApiControllerBase : ControllerBase;
