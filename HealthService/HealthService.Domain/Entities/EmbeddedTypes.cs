using HealthService.Domain.Enums;

namespace HealthService.Domain.Entities;

public class Allergy
{
    public string Substance { get; set; } = string.Empty;
    public AllergySeverity Severity { get; set; }
    public DateTime? NotedAt { get; set; }
}

public class ChronicCondition
{
    public string Name { get; set; } = string.Empty;
    public DateTime? DiagnosedAt { get; set; }
    public string? Notes { get; set; }
}

public class MedicalNote
{
    public string Text { get; set; } = string.Empty;
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class Veterinarian
{
    public string? VetId { get; set; }          // у документі зберігається як "id"
    public string Name { get; set; } = string.Empty;
}

public class VaccineEntry
{
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? BatchNumber { get; set; }
    public double? DoseMl { get; set; }
}

/// <summary>Знімок даних собаки на момент візиту — не оновлюється подіями.</summary>
public class DogSnapshot
{
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public double? WeightKg { get; set; }
}

public class Anesthesia
{
    public string Type { get; set; } = string.Empty;
    public int? DurationMin { get; set; }
}
