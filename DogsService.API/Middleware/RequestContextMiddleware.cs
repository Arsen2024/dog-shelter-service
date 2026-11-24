using DogsService.Domain.Interfaces;
using Serilog.Context;

namespace DogsService.API.Middleware;

/// <summary>
/// Додає до кожного запису логу в межах запиту структуровані властивості
/// CorrelationId (заголовок X-Correlation-Id або новий), UserId та RequestPath.
/// </summary>
public class RequestContextMiddleware(RequestDelegate next)
{
    public const string CorrelationHeader = "X-Correlation-Id";

    public async Task Invoke(HttpContext context, ICurrentUser currentUser)
    {
        var correlationId = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.NewGuid().ToString("N");

        context.Items[CorrelationHeader] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", currentUser.UserName))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value))
        {
            await next(context);
        }
    }
}
