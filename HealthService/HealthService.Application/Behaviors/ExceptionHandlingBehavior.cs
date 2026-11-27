using FluentValidation;
using HealthService.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthService.Application.Behaviors;

public static class ExceptionLogging
{
    /// <summary>Ключ у Exception.Data: виняток уже залоговано на рівні MediatR.</summary>
    public const string LoggedKey = "Logged";
}

/// <summary>
/// Зовнішній behavior: логує винятки на рівні MediatR і відмічає їх як залоговані,
/// щоб глобальний обробник у WebApi не логував їх вдруге.
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse>(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        try
        {
            return await next();
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(
                "[{Request}] rejected by validation, the handler was NOT executed. Errors: {Errors}",
                requestName,
                string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}")));
            ex.Data[ExceptionLogging.LoggedKey] = true;
            throw;
        }
        catch (Exception ex) when (ex is DomainException or NotFoundException or ConflictException)
        {
            logger.LogWarning("[{Request}] failed: {ExceptionType}: {Message}", requestName, ex.GetType().Name, ex.Message);
            ex.Data[ExceptionLogging.LoggedKey] = true;
            throw;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "[{Request}] unhandled exception", requestName);
            ex.Data[ExceptionLogging.LoggedKey] = true;
            throw;
        }
    }
}
