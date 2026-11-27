namespace HealthService.Domain.Exceptions;

/// <summary>Порушення інваріанту домену (недопустиме значення, заборонена дія) → HTTP 400.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>Ресурс не знайдено → HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} with id '{key}' was not found.") { }
}

/// <summary>Конфлікт стану: дублікат, конфлікт версій (оптимістична конкурентність) → HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
