using DogsService.Domain.Enums;
using DogsService.Domain.Exceptions;

namespace DogsService.Domain.Entities;

/// <summary>
/// Собака. Інваріанти перевіряються конструктором і методами сутності (як CatalogItem в eShop):
/// відкритих setter-ів немає, вага змінюється лише через ChangeWeight, статус — через ChangeStatus.
/// </summary>
public class DogEntity
{
    private static readonly Dictionary<DogStatus, DogStatus[]> AllowedTransitions = new()
    {
        [DogStatus.Available] = [DogStatus.Reserved, DogStatus.UnderTreatment],
        [DogStatus.Reserved] = [DogStatus.Available, DogStatus.Adopted],
        [DogStatus.UnderTreatment] = [DogStatus.Available],
        [DogStatus.Adopted] = []
    };

    private readonly List<DogFeatureEntity> _dogFeatures = new();

    // для EF Core
    private DogEntity() { }

    public DogEntity(string name, string color, int tailLength, decimal weight, Guid breedId,
        Guid? kennelId = null, Guid? id = null)
    {
        if (breedId == Guid.Empty)
            throw new DomainException("BreedId is required.");

        Id = id ?? Guid.NewGuid();
        Name = NormalizeName(name);
        Color = NormalizeColor(color);
        TailLength = ValidateTailLength(tailLength);
        Weight = ValidateWeight(weight);
        BreedId = breedId;
        KennelId = kennelId;
        Status = DogStatus.Available;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public int TailLength { get; private set; }
    public decimal Weight { get; private set; }
    public DogStatus Status { get; private set; } = DogStatus.Available;
    public Guid BreedId { get; private set; }
    public Guid? KennelId { get; private set; }

    // аудит і soft delete (заповнюються в AppDbContext)
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    // навігації
    public BreedEntity Breed { get; private set; } = null!;
    public KennelEntity? Kennel { get; private set; }
    public DogProfileEntity? Profile { get; private set; }
    public IReadOnlyCollection<DogFeatureEntity> DogFeatures => _dogFeatures;

    // ---------------- поведінка ----------------

    public void Rename(string name) => Name = NormalizeName(name);

    public void ChangeColor(string color) => Color = NormalizeColor(color);

    public void ChangeWeight(decimal weight) => Weight = ValidateWeight(weight);

    public void ChangeStatus(DogStatus newStatus)
    {
        if (newStatus == Status)
            return;

        if (!AllowedTransitions[Status].Contains(newStatus))
            throw new DomainException($"A dog cannot change status from {Status} to {newStatus}.");

        Status = newStatus;
    }

    public void AssignToKennel(Guid kennelId)
    {
        if (kennelId == Guid.Empty)
            throw new DomainException("KennelId is required.");

        if (Status == DogStatus.Adopted)
            throw new DomainException("An adopted dog cannot be placed in a kennel.");

        KennelId = kennelId;
    }

    public void RemoveFromKennel() => KennelId = null;

    public void SetProfile(DateTime? birthDate, string? gender, string? description, string? temperament)
    {
        if (Profile is null)
            Profile = new DogProfileEntity(this, birthDate, gender, description, temperament);
        else
            Profile.Update(birthDate, gender, description, temperament);
    }

    public void AddFeature(Guid featureId, string? notes = null, DateTime? assignedAt = null)
    {
        if (_dogFeatures.Any(f => f.FeatureId == featureId))
            throw new DomainException("The dog already has this feature.");

        _dogFeatures.Add(new DogFeatureEntity(Id, featureId, notes, assignedAt));
    }

    // ---------------- інваріанти ----------------

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Dog name is required.");

        name = name.Trim();
        if (name.Length > 100)
            throw new DomainException("Dog name cannot be longer than 100 characters.");

        return name;
    }

    private static string NormalizeColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("Dog color is required.");

        color = color.Trim();
        if (color.Length > 100)
            throw new DomainException("Dog color cannot be longer than 100 characters.");

        return color;
    }

    private static int ValidateTailLength(int tailLength)
    {
        if (tailLength < 0)
            throw new DomainException("Tail length cannot be negative.");

        return tailLength;
    }

    private static decimal ValidateWeight(decimal weight)
    {
        if (weight <= 0)
            throw new DomainException("Weight must be greater than zero.");

        if (weight >= 10000)
            throw new DomainException("Weight is too large.");

        if (decimal.Round(weight, 2) != weight)
            throw new DomainException("Weight can have at most 2 decimal places.");

        return weight;
    }
}
