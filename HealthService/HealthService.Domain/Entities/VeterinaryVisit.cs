using HealthService.Domain.Common;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using HealthService.Domain.ValueObjects;
using MongoDB.Bson;

namespace HealthService.Domain.Entities;

/// <summary>
/// Колекція veterinaryVisits — головний агрегат (аналог Review). ГНУЧКА СХЕМА: поля залежать від типу візиту
/// (Checkup: вага й температура; Surgery: анестезія й нотатки операції; Emergency: рівень тріажу 1–5;
/// FollowUp: reference на попередній візит). Порожні поля в документ не записуються.
/// </summary>
public class VeterinaryVisit : BaseEntity
{
    private List<string> _symptoms = new();
    private List<string> _recommendations = new();

    private VeterinaryVisit() { }

    private VeterinaryVisit(ObjectId? id) : base(id) { }

    public static VeterinaryVisit Create(
        Guid dogId,
        string dogName,
        ObjectId medicalRecordId,
        DateTime visitDate,
        VisitType visitType,
        Veterinarian? veterinarian = null,
        IEnumerable<string>? symptoms = null,
        string? diagnosis = null,
        IEnumerable<string>? recommendations = null,
        DogSnapshot? dogSnapshot = null,
        double? weightKg = null,
        double? temperatureC = null,
        Anesthesia? anesthesia = null,
        string? operationNotes = null,
        int? triageLevel = null,
        ObjectId? relatedVisitId = null,
        Money? cost = null,
        ObjectId? id = null)
    {
        if (dogId == Guid.Empty)
            throw new DomainException("DogId is required.");

        if (!Enum.IsDefined(visitType))
            throw new DomainException("Unknown visit type.");

        var date = Guard.ToUtc(visitDate);
        if (date > DateTime.UtcNow.AddDays(1))
            throw new DomainException("Visit date cannot be in the future.");

        if (triageLevel is < 1 or > 5)
            throw new DomainException("Triage level must be between 1 and 5.");

        if (weightKg is <= 0)
            throw new DomainException("Weight must be greater than zero.");

        if (temperatureC is < 30 or > 45)
            throw new DomainException("Temperature must be between 30 and 45 °C.");

        // правила залежно від типу візиту
        if (visitType == VisitType.Surgery && anesthesia is null)
            throw new DomainException("A surgery visit requires anesthesia details.");

        if (visitType == VisitType.Emergency && triageLevel is null)
            throw new DomainException("An emergency visit requires a triage level (1-5).");

        if (visitType == VisitType.FollowUp && relatedVisitId is null)
            throw new DomainException("A follow-up visit requires a related visit.");

        return new VeterinaryVisit(id)
        {
            DogId = dogId,
            DogName = Guard.NotBlank(dogName, "Dog name", 100),
            MedicalRecordId = medicalRecordId,
            RelatedVisitId = relatedVisitId,
            DogSnapshot = dogSnapshot,
            VisitDate = date,
            VisitType = visitType,
            Veterinarian = veterinarian,
            _symptoms = CleanList(symptoms, "Symptom", 200),
            Diagnosis = Guard.Optional(diagnosis, "Diagnosis", 2000),
            _recommendations = CleanList(recommendations, "Recommendation", 500),
            WeightKg = weightKg,
            TemperatureC = temperatureC,
            Anesthesia = anesthesia,
            OperationNotes = Guard.Optional(operationNotes, "Operation notes", 4000),
            TriageLevel = triageLevel,
            Cost = cost
        };
    }

    public Guid DogId { get; private set; }
    public string DogName { get; private set; } = string.Empty;       // денормалізована копія
    public ObjectId MedicalRecordId { get; private set; }             // reference
    public ObjectId? RelatedVisitId { get; private set; }             // reference на попередній візит
    public DogSnapshot? DogSnapshot { get; private set; }             // вбудований знімок (гібрид)
    public DateTime VisitDate { get; private set; }
    public VisitType VisitType { get; private set; }
    public Veterinarian? Veterinarian { get; private set; }
    public string? Diagnosis { get; private set; }

    // специфічні для типу візиту
    public double? WeightKg { get; private set; }
    public double? TemperatureC { get; private set; }
    public Anesthesia? Anesthesia { get; private set; }
    public string? OperationNotes { get; private set; }
    public int? TriageLevel { get; private set; }

    public Money? Cost { get; private set; }

    public IReadOnlyCollection<string> Symptoms => _symptoms;
    public IReadOnlyCollection<string> Recommendations => _recommendations;

    public void Update(
        string? diagnosis, IEnumerable<string>? symptoms, IEnumerable<string>? recommendations,
        string? operationNotes, Money? cost)
    {
        Diagnosis = Guard.Optional(diagnosis, "Diagnosis", 2000);
        _symptoms = CleanList(symptoms, "Symptom", 200);
        _recommendations = CleanList(recommendations, "Recommendation", 500);
        OperationNotes = Guard.Optional(operationNotes, "Operation notes", 4000);
        Cost = cost;
        Touch();
    }

    public void RenameDog(string dogName)
    {
        DogName = Guard.NotBlank(dogName, "Dog name", 100);
        Touch();
    }

    private static List<string> CleanList(IEnumerable<string>? items, string field, int maxLength) =>
        items?.Where(i => !string.IsNullOrWhiteSpace(i))
              .Select(i => Guard.NotBlank(i, field, maxLength))
              .ToList() ?? [];
}
