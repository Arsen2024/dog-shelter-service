using System.Text.RegularExpressions;
using HealthService.Domain.Common;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;

namespace HealthService.Domain.ValueObjects;

// Незмінні Value Objects: усі значення задаються в конструкторі, який перевіряє інваріанти.
// Вбудовуються в документи як вкладені об'єкти (mapping — у Infrastructure).

public sealed class Email : ValueObject
{
    private static readonly Regex Pattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email is required.");

        value = value.Trim().ToLowerInvariant();
        if (value.Length > 256 || !Pattern.IsMatch(value))
            throw new DomainException($"'{value}' is not a valid email address.");

        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

/// <summary>Аналог Author з методички: ветеринар, який провів візит чи вакцинацію.</summary>
public sealed class Veterinarian : ValueObject
{
    public Veterinarian(string? vetId, string name, Email? email = null)
    {
        VetId = Guard.Optional(vetId, "Veterinarian id", 50);
        Name = Guard.NotBlank(name, "Veterinarian name", 100);
        Email = email;
    }

    public string? VetId { get; }          // у документі зберігається як "id"
    public string Name { get; }
    public Email? Email { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return VetId;
        yield return Name;
        yield return Email;
    }
}

public sealed class Money : ValueObject
{
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative.");

        if (amount > 1_000_000m)
            throw new DomainException("Amount cannot exceed 1 000 000.");

        if (decimal.Round(amount, 2) != amount)
            throw new DomainException("Amount can have at most 2 decimal places.");

        currency = (currency ?? string.Empty).Trim();
        if (currency.Length != 3 || !currency.All(char.IsLetter))
            throw new DomainException("Currency must be a 3-letter code (for example UAH).");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public decimal Amount { get; }
    public string Currency { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

/// <summary>Знімок даних собаки на момент візиту — не оновлюється подіями.</summary>
public sealed class DogSnapshot : ValueObject
{
    public DogSnapshot(string name, string? breed, double? weightKg)
    {
        if (weightKg is <= 0)
            throw new DomainException("Snapshot weight must be greater than zero.");

        Name = Guard.NotBlank(name, "Dog name", 100);
        Breed = Guard.Optional(breed, "Breed", 100);
        WeightKg = weightKg;
    }

    public string Name { get; }
    public string? Breed { get; }
    public double? WeightKg { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return Breed;
        yield return WeightKg;
    }
}

public sealed class Anesthesia : ValueObject
{
    public Anesthesia(string type, int? durationMin)
    {
        if (durationMin is < 1 or > 1440)
            throw new DomainException("Anesthesia duration must be between 1 and 1440 minutes.");

        Type = Guard.NotBlank(type, "Anesthesia type", 50);
        DurationMin = durationMin;
    }

    public string Type { get; }
    public int? DurationMin { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type;
        yield return DurationMin;
    }
}

public sealed class Allergy : ValueObject
{
    public Allergy(string substance, AllergySeverity severity, DateTime? notedAt)
    {
        if (!Enum.IsDefined(severity))
            throw new DomainException("Unknown allergy severity.");

        Substance = Guard.NotBlank(substance, "Allergy substance", 200);
        Severity = severity;
        NotedAt = notedAt.HasValue ? Guard.ToUtc(notedAt.Value) : null;
    }

    public string Substance { get; }
    public AllergySeverity Severity { get; }
    public DateTime? NotedAt { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Substance.ToLowerInvariant();
    }
}

public sealed class ChronicCondition : ValueObject
{
    public ChronicCondition(string name, DateTime? diagnosedAt, string? notes)
    {
        Name = Guard.NotBlank(name, "Condition name", 200);
        DiagnosedAt = diagnosedAt.HasValue ? Guard.ToUtc(diagnosedAt.Value) : null;
        Notes = Guard.Optional(notes, "Condition notes", 1000);
    }

    public string Name { get; }
    public DateTime? DiagnosedAt { get; }
    public string? Notes { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
    }
}

public sealed class MedicalNote : ValueObject
{
    public MedicalNote(string text, string? author, DateTime createdAt)
    {
        Text = Guard.NotBlank(text, "Note text", 2000);
        Author = Guard.Optional(author, "Note author", 100);
        CreatedAt = Guard.ToUtc(createdAt);
    }

    public string Text { get; }
    public string? Author { get; }
    public DateTime CreatedAt { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Text;
        yield return Author;
        yield return CreatedAt;
    }
}

public sealed class VaccineEntry : ValueObject
{
    public VaccineEntry(string name, string? manufacturer, string? batchNumber, double? doseMl)
    {
        if (doseMl is <= 0)
            throw new DomainException("Vaccine dose must be greater than zero.");

        Name = Guard.NotBlank(name, "Vaccine name", 100);
        Manufacturer = Guard.Optional(manufacturer, "Manufacturer", 100);
        BatchNumber = Guard.Optional(batchNumber, "Batch number", 50);
        DoseMl = doseMl;
    }

    public string Name { get; }
    public string? Manufacturer { get; }
    public string? BatchNumber { get; }
    public double? DoseMl { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name.ToLowerInvariant();
        yield return BatchNumber;
    }
}
