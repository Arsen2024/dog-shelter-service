using System.Net;
using System.Text.Json;
using AdoptionsService.Domain.Exceptions;
using FluentValidation;

namespace AdoptionsService.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate _next,
    ILogger<ExceptionHandlingMiddleware> _logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, body) = ex switch
            {
                ValidationException ve => (HttpStatusCode.BadRequest, (object)new
                {
                    error = "Validation failed",
                    details = ve.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage })
                }),
                NotFoundException => (HttpStatusCode.NotFound, new { error = ex.Message }),
                BusinessRuleException => (HttpStatusCode.Conflict, new { error = ex.Message }),
                _ => (HttpStatusCode.InternalServerError, new { error = "Internal server error" })
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception");
            else
                _logger.LogWarning("Handled {Type}: {Message}", ex.GetType().Name, ex.Message);

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(body));
        }
    }
}
