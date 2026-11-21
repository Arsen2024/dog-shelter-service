using HealthService.Domain.Interfaces;

namespace HealthService.API.Middleware;

public class HttpCurrentUser(IHttpContextAccessor _accessor) : ICurrentUser
{
    public string? UserName
    {
        get
        {
            var header = _accessor.HttpContext?.Request.Headers["X-User-Name"].FirstOrDefault();
            return string.IsNullOrWhiteSpace(header) ? "system" : header.Trim();
        }
    }
}
