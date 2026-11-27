using HealthService.Domain.Common;
using HealthService.Domain.Exceptions;
using HealthService.Domain.ValueObjects;
using MongoDB.Bson;

namespace HealthService.Domain.Entities;

/// <summary>Колекція medicalRecords: одна картка на собаку. Алергії, хронічні хвороби й нотатки — вбудовані масиви.</summary>
public class MedicalRecord : BaseEntity
{
    // приватні поля — BSON-мапінг налаштовується в Infrastructure (MapField)
    private List<Allergy> _allergies = new();
    private List<ChronicCondition> _chronicConditions = new();
    private List<MedicalNote> _notes = new();

    private MedicalRecord() { }

    private MedicalRecord(ObjectId? id) : base(id) { }

    public static MedicalRecord Create(Guid dogId, string dogName, string? bloodType = null, ObjectId? id = null)
    {
        if (dogId == Guid.Empty)
            throw new DomainException("DogId is required.");

        return new MedicalRecord(id)
        {
            DogId = dogId,
            DogName = Guard.NotBlank(dogName, "Dog name", 100),
            BloodType = Guard.Optional(bloodType, "Blood type", 50)
        };
    }

    public Guid DogId { get; private set; }                 // логічне посилання на DogsDb (без FK)
    public string DogName { get; private set; } = string.Empty;   // денормалізована копія
    public string? BloodType { get; private set; }

    public IReadOnlyCollection<Allergy> Allergies => _allergies;
    public IReadOnlyCollection<ChronicCondition> ChronicConditions => _chronicConditions;
    public IReadOnlyCollection<MedicalNote> Notes => _notes;

    public void AddAllergy(Allergy allergy)
    {
        if (_allergies.Contains(allergy))
            throw new DomainException($"Allergy to '{allergy.Substance}' is already recorded.");

        _allergies.Add(allergy);
        Touch();
    }

    public void AddChronicCondition(ChronicCondition condition)
    {
        if (_chronicConditions.Contains(condition))
            throw new DomainException($"Condition '{condition.Name}' is already recorded.");

        _chronicConditions.Add(condition);
        Touch();
    }

    public void AddNote(MedicalNote note)
    {
        _notes.Add(note);
        Touch();
    }

    public void ChangeBloodType(string? bloodType)
    {
        BloodType = Guard.Optional(bloodType, "Blood type", 50);
        Touch();
    }

    public void RenameDog(string dogName)
    {
        DogName = Guard.NotBlank(dogName, "Dog name", 100);
        Touch();
    }
}
