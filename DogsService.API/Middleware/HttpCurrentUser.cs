using DogsService.Domain.Interfaces;

namespace DogsService.API.Middleware;

/// <summary>
/// Спрощена ідентифікація користувача для аудиту: заголовок X-User-Name, інакше "system".
/// (Справжня автентифікація — окрема тема.)
/// </summary>
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
