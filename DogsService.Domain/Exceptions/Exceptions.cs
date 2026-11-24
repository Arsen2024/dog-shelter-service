namespace DogsService.Domain.Exceptions;

/// <summary>Ресурс не знайдено → HTTP 404.</summary>
public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string entity, object key)
        : base($"{entity} with id '{key}' was not found.") { }
}

/// <summary>Конфлікт з поточним станом даних (дублікат, переповнений вольєр) → HTTP 409.</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Порушення інваріанту сутності (недопустимий перехід статусу тощо) → HTTP 409.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
