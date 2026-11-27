using HealthService.Domain.Common;
using HealthService.Domain.Exceptions;
using HealthService.Domain.ValueObjects;
using MongoDB.Bson;

namespace HealthService.Domain.Entities;

/// <summary>Колекція vaccinations: вкладений масив вакцин + reference на medicalRecords (гібридний підхід).</summary>
public class Vaccination : BaseEntity
{
    private List<VaccineEntry> _vaccines = new();

    private Vaccination() { }

    private Vaccination(ObjectId? id) : base(id) { }

    public static Vaccination Create(
        Guid dogId, string dogName, ObjectId medicalRecordId, DateTime vaccinatedAt, DateTime? nextDueAt,
        Veterinarian? veterinarian, IEnumerable<VaccineEntry> vaccines, ObjectId? id = null)
    {
        if (dogId == Guid.Empty)
            throw new DomainException("DogId is required.");

        var list = vaccines?.ToList() ?? [];
        if (list.Count == 0)
            throw new DomainException("At least one vaccine is required.");

        var vaccinated = Guard.ToUtc(vaccinatedAt);
        if (vaccinated > DateTime.UtcNow.AddDays(1))
            throw new DomainException("Vaccination date cannot be in the future.");

        DateTime? next = nextDueAt.HasValue ? Guard.ToUtc(nextDueAt.Value) : null;
        if (next.HasValue && next.Value <= vaccinated)
            throw new DomainException("NextDueAt must be later than the vaccination date.");

        return new Vaccination(id)
        {
            DogId = dogId,
            DogName = Guard.NotBlank(dogName, "Dog name", 100),
            MedicalRecordId = medicalRecordId,
            VaccinatedAt = vaccinated,
            NextDueAt = next,
            Veterinarian = veterinarian,
            _vaccines = list
        };
    }

    public Guid DogId { get; private set; }
    public string DogName { get; private set; } = string.Empty;
    public ObjectId MedicalRecordId { get; private set; }          // reference
    public DateTime VaccinatedAt { get; private set; }
    public DateTime? NextDueAt { get; private set; }
    public Veterinarian? Veterinarian { get; private set; }

    public IReadOnlyCollection<VaccineEntry> Vaccines => _vaccines;

    public void Reschedule(DateTime? nextDueAt)
    {
        DateTime? next = nextDueAt.HasValue ? Guard.ToUtc(nextDueAt.Value) : null;
        if (next.HasValue && next.Value <= VaccinatedAt)
            throw new DomainException("NextDueAt must be later than the vaccination date.");

        NextDueAt = next;
        Touch();
    }

    public void RenameDog(string dogName)
    {
        DogName = Guard.NotBlank(dogName, "Dog name", 100);
        Touch();
    }
}
