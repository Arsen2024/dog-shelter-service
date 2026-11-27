using FluentValidation;
using HealthService.Application.Behaviors;
using HealthService.Application.Exceptions;
using HealthService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HealthService.API.Middleware;

/// <summary>
/// Єдиний глобальний обробник винятків → ProblemDetails (RFC 7807) з traceId.
///   NotFoundException → 404; ValidationException → 400 (ValidationProblemDetails); DomainException → 400;
///   ConflictException (у т. ч. конфлікт версій і DuplicateKey) → 409; ServiceUnavailableException
///   (MongoConnectionException / таймаут вибору сервера) → 503; решта → 500.
/// MongoDB-винятки перетворює на ці типи шар Infrastructure (MongoGuard), тож WebApi не залежить від драйвера.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var problem = BuildProblem(exception);
        problem.Instance = httpContext.Request.Path;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        // виняток уже залоговано ExceptionHandlingBehavior — вдруге не логуємо
        if (!exception.Data.Contains(ExceptionLogging.LoggedKey))
        {
            if (problem.Status >= 500)
                logger.LogError(exception, "Unhandled exception");
            else
                logger.LogWarning("Request rejected with {StatusCode}: {Message}", problem.Status, exception.Message);
        }

        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            problem, problem.GetType(), options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }

    private ProblemDetails BuildProblem(Exception exception)
    {
        switch (exception)
        {
            case ValidationException validation:
                var errors = validation.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).Distinct().ToArray());

                return new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred."
                };

            case DomainException:
                return Create(StatusCodes.Status400BadRequest, "Bad Request", exception.Message);

            case NotFoundException:
                return Create(StatusCodes.Status404NotFound, "Not Found", exception.Message);

            case ConflictException:
                return Create(StatusCodes.Status409Conflict, "Conflict", exception.Message);

            case ServiceUnavailableException:
                return Create(StatusCodes.Status503ServiceUnavailable, "Service Unavailable", exception.Message);

            default:
                return Create(StatusCodes.Status500InternalServerError, "Internal Server Error",
                    environment.IsDevelopment()
                        ? exception.ToString()
                        : "An unexpected error occurred. Use the traceId when contacting support.");
        }
    }

    private static ProblemDetails Create(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };
}
