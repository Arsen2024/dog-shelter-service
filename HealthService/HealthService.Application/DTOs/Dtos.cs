using HealthService.Domain.Enums;

namespace HealthService.Application.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext { get; init; }
    public bool HasPrevious { get; init; }

    public static PagedResult<T> Create(List<T> items, long totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<T>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            HasPrevious = page > 1
        };
    }
}

// ---------------- спільні вкладені об'єкти ----------------

public class MoneyDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class VeterinarianDto
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class DogSnapshotDto
{
    public string Name { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public double? WeightKg { get; set; }
}

public class AnesthesiaDto
{
    public string Type { get; set; } = string.Empty;
    public int? DurationMin { get; set; }
}

// ---------------- візити ----------------

public class VisitDto
{
    public string Id { get; set; } = string.Empty;
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string MedicalRecordId { get; set; } = string.Empty;
    public string? RelatedVisitId { get; set; }
    public DogSnapshotDto? DogSnapshot { get; set; }
    public DateTime VisitDate { get; set; }
    public VisitType VisitType { get; set; }
    public VeterinarianDto? Veterinarian { get; set; }
    public List<string> Symptoms { get; set; } = [];
    public string? Diagnosis { get; set; }
    public List<string> Recommendations { get; set; } = [];
    public double? WeightKg { get; set; }
    public double? TemperatureC { get; set; }
    public AnesthesiaDto? Anesthesia { get; set; }
    public string? OperationNotes { get; set; }
    public int? TriageLevel { get; set; }
    public MoneyDto? Cost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>Версія документа — передається назад в UpdateVisit (оптимістична конкурентність).</summary>
    public long Version { get; set; }
}

public class VisitListItemDto
{
    public string Id { get; set; } = string.Empty;
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }
    public VisitType VisitType { get; set; }
    public string? VeterinarianName { get; set; }
    public string? Diagnosis { get; set; }
    public int? TriageLevel { get; set; }
    public long Version { get; set; }
}

// ---------------- медична картка ----------------

public class AllergyDto
{
    public string Substance { get; set; } = string.Empty;
    public AllergySeverity Severity { get; set; }
    public DateTime? NotedAt { get; set; }
}

public class ChronicConditionDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime? DiagnosedAt { get; set; }
    public string? Notes { get; set; }
}

public class MedicalNoteDto
{
    public string Text { get; set; } = string.Empty;
    public string? Author { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MedicalRecordDto
{
    public string Id { get; set; } = string.Empty;
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string? BloodType { get; set; }
    public List<AllergyDto> Allergies { get; set; } = [];
    public List<ChronicConditionDto> ChronicConditions { get; set; } = [];
    public List<MedicalNoteDto> Notes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long Version { get; set; }
}

// ---------------- вакцинації ----------------

public class VaccineDto
{
    public string Name { get; set; } = string.Empty;
    public string? Manufacturer { get; set; }
    public string? BatchNumber { get; set; }
    public double? DoseMl { get; set; }
}

public class VaccinationDto
{
    public string Id { get; set; } = string.Empty;
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string MedicalRecordId { get; set; } = string.Empty;
    public DateTime VaccinatedAt { get; set; }
    public DateTime? NextDueAt { get; set; }
    public VeterinarianDto? Veterinarian { get; set; }
    public List<VaccineDto> Vaccines { get; set; } = [];
    public long Version { get; set; }
}

public class OverdueVaccinationDto
{
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public DateTime LastVaccinatedAt { get; set; }
    public DateTime NextDueAt { get; set; }
}
