using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthService.Application.Behaviors;

/// <summary>Логує запит до виклику обробника й відповідь після нього. Якщо обробник не виконувався — рядка «Handled» немає.</summary>
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private const int MaxLength = 800;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling {Request} {Payload}", requestName, Serialize(request));

        var response = await next();

        logger.LogInformation("Handled {Request} -> {ResponseType} {Response}",
            requestName, typeof(TResponse).Name, Serialize(response));

        return response;
    }

    private static string Serialize(object? value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            return json.Length <= MaxLength ? json : json[..MaxLength] + "...";
        }
        catch
        {
            return "<not serializable>";
        }
    }
}
