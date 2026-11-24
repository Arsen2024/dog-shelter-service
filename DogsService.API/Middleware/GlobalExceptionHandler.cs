using DogsService.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DogsService.API.Middleware;

/// <summary>
/// Єдиний глобальний обробник винятків → ProblemDetails (RFC 7807).
///   EntityNotFoundException → 404; ValidationException (FluentValidation) → 400 (ValidationProblemDetails);
///   ConflictException, DomainException, конфлікти версій і порушення UNIQUE → 409; решта → 500.
/// Кожен виняток логується рівно один раз (тут). Деталі 500-ї помилки віддаються лише в Development.
/// </summary>
public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // клієнт скасував запит — це не помилка сервісу
        if (exception is OperationCanceledException)
        {
            httpContext.Response.StatusCode = 499;
            return true;
        }

        var problem = BuildProblem(exception);

        problem.Instance = httpContext.Request.Path;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        if (httpContext.Items[RequestContextMiddleware.CorrelationHeader] is string correlationId)
            problem.Extensions["correlationId"] = correlationId;

        if (problem.Status >= 500)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning("Request rejected with {StatusCode}: {Message}", problem.Status, exception.Message);

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

            case EntityNotFoundException:
                return Create(StatusCodes.Status404NotFound, "Not Found", exception.Message);

            case ConflictException or DomainException:
                return Create(StatusCodes.Status409Conflict, "Conflict", exception.Message);

            case DbUpdateConcurrencyException:
                return Create(StatusCodes.Status409Conflict, "Conflict",
                    "The record was modified by another user. Reload it and try again.");

            case DbUpdateException { InnerException: SqlException { Number: 2601 or 2627 } }:
                return Create(StatusCodes.Status409Conflict, "Conflict",
                    "A record with the same unique value already exists.");

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
