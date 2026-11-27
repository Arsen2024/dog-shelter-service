using HealthService.Application.DTOs;
using HealthService.Domain.Enums;
using MongoDB.Bson;

namespace HealthService.Infrastructure.Queries;

/// <summary>Перетворення проєкцій (BsonDocument) одразу в DTO — без матеріалізації доменних сутностей.</summary>
internal static class DocumentMapper
{
    public static VeterinarianDto? Veterinarian(BsonDocument? d) => d is null
        ? null
        : new VeterinarianDto { Id = d.GetStringOrNull("id"), Name = d.GetStringOrNull("name") ?? string.Empty, Email = d.GetStringOrNull("email") };

    public static VisitDto Visit(BsonDocument d)
    {
        var snapshot = d.GetDocOrNull("dogSnapshot");
        var anesthesia = d.GetDocOrNull("anesthesia");
        var cost = d.GetDocOrNull("cost");

        return new VisitDto
        {
            Id = d["_id"].AsObjectId.ToString(),
            DogId = Guid.Parse(d["dogId"].AsString),
            DogName = d["dogName"].AsString,
            MedicalRecordId = d["medicalRecordId"].AsObjectId.ToString(),
            RelatedVisitId = d.GetObjectIdOrNull("relatedVisitId")?.ToString(),
            DogSnapshot = snapshot is null ? null : new DogSnapshotDto
            {
                Name = snapshot.GetStringOrNull("name") ?? string.Empty,
                Breed = snapshot.GetStringOrNull("breed"),
                WeightKg = snapshot.GetDoubleOrNull("weightKg")
            },
            VisitDate = d["visitDate"].ToUniversalTime(),
            VisitType = Enum.Parse<VisitType>(d["visitType"].AsString),
            Veterinarian = Veterinarian(d.GetDocOrNull("veterinarian")),
            Symptoms = d.GetStrings("symptoms"),
            Diagnosis = d.GetStringOrNull("diagnosis"),
            Recommendations = d.GetStrings("recommendations"),
            WeightKg = d.GetDoubleOrNull("weightKg"),
            TemperatureC = d.GetDoubleOrNull("temperatureC"),
            Anesthesia = anesthesia is null ? null : new AnesthesiaDto
            {
                Type = anesthesia.GetStringOrNull("type") ?? string.Empty,
                DurationMin = anesthesia.GetIntOrNull("durationMin")
            },
            OperationNotes = d.GetStringOrNull("operationNotes"),
            TriageLevel = d.GetIntOrNull("triageLevel"),
            Cost = cost is null ? null : new MoneyDto
            {
                Amount = cost.GetDecimalOrNull("amount") ?? 0,
                Currency = cost.GetStringOrNull("currency") ?? string.Empty
            },
            CreatedAt = d.GetDateOrNull("createdAt") ?? default,
            UpdatedAt = d.GetDateOrNull("updatedAt"),
            Version = d.GetLong("version")
        };
    }

    public static VisitListItemDto VisitListItem(BsonDocument d) => new()
    {
        Id = d["_id"].AsObjectId.ToString(),
        DogId = Guid.Parse(d["dogId"].AsString),
        DogName = d["dogName"].AsString,
        VisitDate = d["visitDate"].ToUniversalTime(),
        VisitType = Enum.Parse<VisitType>(d["visitType"].AsString),
        VeterinarianName = d.GetDocOrNull("veterinarian")?.GetStringOrNull("name"),
        Diagnosis = d.GetStringOrNull("diagnosis"),
        TriageLevel = d.GetIntOrNull("triageLevel"),
        Version = d.GetLong("version")
    };

    public static MedicalRecordDto MedicalRecord(BsonDocument d) => new()
    {
        Id = d["_id"].AsObjectId.ToString(),
        DogId = Guid.Parse(d["dogId"].AsString),
        DogName = d["dogName"].AsString,
        BloodType = d.GetStringOrNull("bloodType"),
        Allergies = d.GetDocs("allergies").Select(a => new AllergyDto
        {
            Substance = a.GetStringOrNull("substance") ?? string.Empty,
            Severity = Enum.Parse<AllergySeverity>(a.GetStringOrNull("severity") ?? nameof(AllergySeverity.Mild)),
            NotedAt = a.GetDateOrNull("notedAt")
        }).ToList(),
        ChronicConditions = d.GetDocs("chronicConditions").Select(c => new ChronicConditionDto
        {
            Name = c.GetStringOrNull("name") ?? string.Empty,
            DiagnosedAt = c.GetDateOrNull("diagnosedAt"),
            Notes = c.GetStringOrNull("notes")
        }).ToList(),
        Notes = d.GetDocs("notes").Select(n => new MedicalNoteDto
        {
            Text = n.GetStringOrNull("text") ?? string.Empty,
            Author = n.GetStringOrNull("author"),
            CreatedAt = n.GetDateOrNull("createdAt") ?? default
        }).ToList(),
        CreatedAt = d.GetDateOrNull("createdAt") ?? default,
        UpdatedAt = d.GetDateOrNull("updatedAt"),
        Version = d.GetLong("version")
    };

    public static VaccinationDto Vaccination(BsonDocument d) => new()
    {
        Id = d["_id"].AsObjectId.ToString(),
        DogId = Guid.Parse(d["dogId"].AsString),
        DogName = d["dogName"].AsString,
        MedicalRecordId = d["medicalRecordId"].AsObjectId.ToString(),
        VaccinatedAt = d["vaccinatedAt"].ToUniversalTime(),
        NextDueAt = d.GetDateOrNull("nextDueAt"),
        Veterinarian = Veterinarian(d.GetDocOrNull("veterinarian")),
        Vaccines = d.GetDocs("vaccines").Select(v => new VaccineDto
        {
            Name = v.GetStringOrNull("name") ?? string.Empty,
            Manufacturer = v.GetStringOrNull("manufacturer"),
            BatchNumber = v.GetStringOrNull("batchNumber"),
            DoseMl = v.GetDoubleOrNull("doseMl")
        }).ToList(),
        Version = d.GetLong("version")
    };
}
