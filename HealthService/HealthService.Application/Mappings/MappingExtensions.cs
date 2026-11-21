using HealthService.Application.DTOs;
using HealthService.Domain.Entities;
using HealthService.Domain.Models;

namespace HealthService.Application.Mappings;

public static class MappingExtensions
{
    public static MedicalRecordDto ToDto(this MedicalRecord r) => new(
        r.Id, r.DogId, r.DogName, r.BloodType,
        r.Allergies.Select(a => new AllergyDto(a.Substance, a.Severity, a.NotedAt)).ToList(),
        r.ChronicConditions.Select(c => new ChronicConditionDto(c.Name, c.DiagnosedAt, c.Notes)).ToList(),
        r.Notes.Select(n => new MedicalNoteDto(n.Text, n.Author, n.CreatedAt)).ToList(),
        r.CreatedAt, r.UpdatedAt);

    private static VeterinarianDto? ToDto(this Veterinarian? v) => v is null ? null : new(v.VetId, v.Name);

    public static VaccinationDto ToDto(this Vaccination v) => new(
        v.Id, v.DogId, v.DogName, v.MedicalRecordId, v.VaccinatedAt, v.NextDueAt, v.Veterinarian.ToDto(),
        v.Vaccines.Select(x => new VaccineDto(x.Name, x.Manufacturer, x.BatchNumber, x.DoseMl)).ToList());

    public static OverdueVaccinationDto ToDto(this OverdueVaccination o) =>
        new(o.DogId, o.DogName, o.LastVaccinatedAt, o.NextDueAt);

    public static VisitDto ToDto(this VeterinaryVisit v) => new(
        v.Id, v.DogId, v.DogName, v.MedicalRecordId, v.RelatedVisitId,
        v.DogSnapshot is null ? null : new DogSnapshotDto(v.DogSnapshot.Name, v.DogSnapshot.Breed, v.DogSnapshot.WeightKg),
        v.VisitDate, v.VisitType, v.Veterinarian.ToDto(),
        v.Symptoms, v.Diagnosis, v.Recommendations,
        v.WeightKg, v.TemperatureC,
        v.Anesthesia is null ? null : new AnesthesiaDto(v.Anesthesia.Type, v.Anesthesia.DurationMin),
        v.OperationNotes, v.TriageLevel);

    /// <summary>Гарантує UTC для дат, отриманих з JSON (Unspecified вважаємо UTC).</summary>
    public static DateTime ToUtc(this DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
