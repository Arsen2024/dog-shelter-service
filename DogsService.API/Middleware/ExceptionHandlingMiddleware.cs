using DogsService.Application.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
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

            var (status, message) = ex switch
            {
                ValidationException => (HttpStatusCode.BadRequest, ex.Message),
                NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                DuplicateDogNameException => (HttpStatusCode.Conflict, ex.Message),
                DbUpdateConcurrencyException => (HttpStatusCode.Conflict,
                    "The record was modified by another user. Reload and try again."),
                _ => (HttpStatusCode.InternalServerError, "Internal server error")
            };

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
        }
    }
}
