using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HealthService.Application.Behaviors;

/// <summary>Попереджає в лог про повільні обробники (понад 500 мс).</summary>
public class PerformanceBehavior<TRequest, TResponse>(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private const int ThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next();
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > ThresholdMs)
                logger.LogWarning("Long running request {Request} took {ElapsedMs} ms (threshold {Threshold} ms)",
                    typeof(TRequest).Name, stopwatch.ElapsedMilliseconds, ThresholdMs);
        }
    }
}
