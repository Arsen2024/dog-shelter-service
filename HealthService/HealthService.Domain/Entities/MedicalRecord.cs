namespace HealthService.Domain.Entities;

/// <summary>Колекція medicalRecords: одна картка на собаку. Alergies/ChronicConditions/Notes — вбудовані (embedding).</summary>
public class MedicalRecord
{
    public string Id { get; set; } = string.Empty;                 // ObjectId (рядок)
    public Guid DogId { get; set; }                                // логічне посилання на DogsDb (без FK)
    public string DogName { get; set; } = string.Empty;            // денормалізована копія
    public string? BloodType { get; set; }
    public List<Allergy> Allergies { get; set; } = [];
    public List<ChronicCondition> ChronicConditions { get; set; } = [];
    public List<MedicalNote> Notes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
