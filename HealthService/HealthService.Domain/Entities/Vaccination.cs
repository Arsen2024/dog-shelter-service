namespace HealthService.Domain.Entities;

/// <summary>Колекція vaccinations: вкладений масив Vaccines + reference на medicalRecords (гібридний підхід).</summary>
public class Vaccination
{
    public string Id { get; set; } = string.Empty;
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string MedicalRecordId { get; set; } = string.Empty;   // reference (ObjectId)
    public DateTime VaccinatedAt { get; set; }
    public DateTime? NextDueAt { get; set; }
    public Veterinarian? Veterinarian { get; set; }
    public List<VaccineEntry> Vaccines { get; set; } = [];
}
