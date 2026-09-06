using System.Text.Json;
using Messenger.Api.Configuration;
using Messenger.Api.Data;
using Messenger.Api.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Падаем на старте, а не на первом запросе к БД: формат строки сменился вместе
// со стеком, и молча не подошедшее значение искать дороже.
var connectionString = builder.Configuration.GetPostgresConnectionString();

// Соединение открывается лениво, на первом запросе к БД: регистрация контекста
// старт приложения к доступности Postgres не привязывает.
builder.Services.AddDbContext<MessengerDbContext>(options => options.UseNpgsql(connectionString));

builder.Services
    .AddControllers()
    .AddJsonOptions(ConfigureJson)
    .ConfigureApiBehaviorOptions(options =>
    {
        // Контракт требует { reason }, а не ProblemDetails.
        options.SuppressMapClientErrors = true;
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(new ErrorResponse(DescribeValidationErrors(context.ModelState)));
    });

// WriteAsJsonAsync берет эти настройки, а не MVC-шные: без них тело ошибки
// сериализовалось бы по другим правилам, чем тело ответа контроллера.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();

var app = builder.Build();

app.UseExceptionHandler(new ExceptionHandlerOptions
{
    // UnhandledExceptionHandler отвечает сам. Пустой делегат нужен только
    // чтобы middleware не потребовал ExceptionHandlingPath или ProblemDetails.
    ExceptionHandler = _ => Task.CompletedTask,
});

// Ответы без тела - 404 и прочие - тоже приводим к контракту. 401 остается
// пустым: контракт тела для него не описывает.
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        return;
    }

    var reason = ReasonPhrases.GetReasonPhrase(response.StatusCode);
    await response.WriteAsJsonAsync(
        new ErrorResponse(string.IsNullOrEmpty(reason) ? "Request failed" : reason));
});

app.MapControllers();

app.Run();

static void ConfigureJson(Microsoft.AspNetCore.Mvc.JsonOptions options) =>
    // Контракт снаружи snake_case (first_name), C# внутри PascalCase.
    // Политика задается один раз вместо [JsonPropertyName] на каждом поле.
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

static string DescribeValidationErrors(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
{
    var messages = modelState.Values
        .SelectMany(entry => entry.Errors)
        .Select(error => error.ErrorMessage)
        .Where(message => !string.IsNullOrWhiteSpace(message))
        .Distinct()
        .ToArray();

    return messages.Length > 0 ? string.Join("; ", messages) : "Invalid request";
}

// WebApplicationFactory в e2e-тестах требует доступного типа точки входа,
// а при top-level statements он генерируется internal.
public partial class Program;
