using AdoptionsService.Domain.Interfaces;

namespace AdoptionsService.API.Middleware;

/// <summary>Спрощена ідентифікація для аудиту: заголовок X-User-Name, інакше "system".</summary>
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
