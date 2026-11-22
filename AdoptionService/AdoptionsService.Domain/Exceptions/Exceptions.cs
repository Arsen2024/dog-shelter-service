namespace AdoptionsService.Domain.Exceptions;

/// <summary>Ресурс не існує → HTTP 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} with id '{key}' was not found.") { }
}

/// <summary>Порушення бізнес-правила (недопустимий перехід статусу, дублікат, конфлікт версій) → HTTP 409.</summary>
public class BusinessConflictException : Exception
{
    public BusinessConflictException(string message) : base(message) { }
}

/// <summary>Помилка валідації, яку неможливо виразити атрибутами DTO → HTTP 400.</summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
