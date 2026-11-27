namespace HealthService.Application.Abstractions;

/// <summary>Поточний користувач (для авторства нотаток). Реалізація — у WebApi.</summary>
public interface ICurrentUser
{
    string? UserName { get; }
}
