using HealthService.Application.Abstractions;

namespace HealthService.API.Middleware;

/// <summary>Спрощена ідентифікація для авторства нотаток: заголовок X-User-Name, інакше "system".</summary>
public class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public string? UserName
    {
        get
        {
            var header = accessor.HttpContext?.Request.Headers["X-User-Name"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(header) ? "system" : header.Trim();
        }
    }
}
