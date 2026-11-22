using AdoptionsService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AdoptionsService.API.Middleware;

/// <summary>
/// Єдиний глобальний обробник винятків (IExceptionHandler) → ProblemDetails (RFC 7807) з traceId.
///   ValidationException → 400, NotFoundException → 404, BusinessConflictException → 409, решта → 500.
/// Деталі 500-ї помилки клієнту не віддаються; у лог потрапляє повний виняток з traceId
/// (рядок підключення та інші секрети не логуються).
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // клієнт скасував запит — це не помилка сервісу
        if (exception is OperationCanceledException)
        {
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Bad Request"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            BusinessConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        var traceId = httpContext.TraceIdentifier;

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
        else
            logger.LogWarning("{ExceptionType}: {Message}. TraceId: {TraceId}", exception.GetType().Name, exception.Message, traceId);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Use the traceId when contacting support."
                : exception.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);

        return true;
    }
}
