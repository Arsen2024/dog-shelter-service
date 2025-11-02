using DogsService.Application.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace DogsService.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate _nextMiddleware,
    ILogger<ExceptionHandlingMiddleware> _logger
    )
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _nextMiddleware(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                DuplicateDogNameException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
    }
}