using HealthService.Domain.Enums;

namespace HealthService.Domain.Entities;

/// <summary>
/// Колекція veterinaryVisits — ГНУЧКА СХЕМА: спільні поля + поля, специфічні для типу візиту
/// (Checkup: WeightKg/TemperatureC; Surgery: Anesthesia/OperationNotes; Emergency: TriageLevel;
/// FollowUp: RelatedVisitId). Порожні (null) поля в документ не записуються.
/// </summary>
public class VeterinaryVisit
{
    public string Id { get; set; } = string.Empty;
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string MedicalRecordId { get; set; } = string.Empty;   // reference
    public string? RelatedVisitId { get; set; }                   // reference на попередній візит
    public DogSnapshot? DogSnapshot { get; set; }                 // вбудований знімок (гібрид)
    public DateTime VisitDate { get; set; }
    public VisitType VisitType { get; set; }
    public Veterinarian? Veterinarian { get; set; }
    public List<string> Symptoms { get; set; } = [];
    public string? Diagnosis { get; set; }
    public List<string> Recommendations { get; set; } = [];

    // Специфічні для типу
    public double? WeightKg { get; set; }
    public double? TemperatureC { get; set; }
    public Anesthesia? Anesthesia { get; set; }
    public string? OperationNotes { get; set; }
    public int? TriageLevel { get; set; }
}
