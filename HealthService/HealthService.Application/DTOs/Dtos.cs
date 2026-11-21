using HealthService.Domain.Enums;

namespace HealthService.Application.DTOs;

public record AllergyDto(string Substance, AllergySeverity Severity, DateTime? NotedAt);
public record ChronicConditionDto(string Name, DateTime? DiagnosedAt, string? Notes);
public record MedicalNoteDto(string Text, string? Author, DateTime CreatedAt);

public record MedicalRecordDto(
    string Id, Guid DogId, string DogName, string? BloodType,
    List<AllergyDto> Allergies, List<ChronicConditionDto> ChronicConditions, List<MedicalNoteDto> Notes,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record VeterinarianDto(string? Id, string Name);
public record VaccineDto(string Name, string? Manufacturer, string? BatchNumber, double? DoseMl);

public record VaccinationDto(
    string Id, Guid DogId, string DogName, string MedicalRecordId,
    DateTime VaccinatedAt, DateTime? NextDueAt, VeterinarianDto? Veterinarian, List<VaccineDto> Vaccines);

public record OverdueVaccinationDto(Guid DogId, string DogName, DateTime LastVaccinatedAt, DateTime NextDueAt);

public record DogSnapshotDto(string Name, string? Breed, double? WeightKg);
public record AnesthesiaDto(string Type, int? DurationMin);

public record VisitDto(
    string Id, Guid DogId, string DogName, string MedicalRecordId, string? RelatedVisitId,
    DogSnapshotDto? DogSnapshot, DateTime VisitDate, VisitType VisitType, VeterinarianDto? Veterinarian,
    List<string> Symptoms, string? Diagnosis, List<string> Recommendations,
    double? WeightKg, double? TemperatureC, AnesthesiaDto? Anesthesia, string? OperationNotes, int? TriageLevel);
