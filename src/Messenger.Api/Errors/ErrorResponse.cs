namespace Messenger.Api.Errors;

/// <summary>
/// Тело ошибки из контракта: <c>{ "reason": "..." }</c> и ничего больше.
/// ProblemDetails здесь не годится - он всегда сериализуется в camelCase,
/// независимо от политики именования, и несет лишние поля.
/// </summary>
/// <param name="Reason">Текст ошибки. Внутренности наружу не попадают.</param>
public sealed record ErrorResponse(string Reason);
