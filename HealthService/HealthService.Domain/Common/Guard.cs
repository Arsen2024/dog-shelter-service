using HealthService.Domain.Exceptions;

namespace HealthService.Domain.Common;

internal static class Guard
{
    public static string NotBlank(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field} is required.");

        value = value.Trim();
        if (value.Length > maxLength)
            throw new DomainException($"{field} cannot be longer than {maxLength} characters.");

        return value;
    }

    public static string? Optional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim();
        if (value.Length > maxLength)
            throw new DomainException($"{field} cannot be longer than {maxLength} characters.");

        return value;
    }

    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
