namespace HealthService.Application.Exceptions;

/// <summary>Зовнішній ресурс (MongoDB) недоступний → HTTP 503.</summary>
public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message, Exception? inner = null) : base(message, inner) { }
}
