namespace AdoptionsService.Domain.Interfaces;

/// <summary>Поточний користувач (для аудиту). Реалізація — у Web-шарі.</summary>
public interface ICurrentUser
{
    string? UserName { get; }
}
