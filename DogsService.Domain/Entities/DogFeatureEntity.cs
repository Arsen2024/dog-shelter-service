using DogsService.Domain.Exceptions;

namespace DogsService.Domain.Entities;

/// <summary>Проміжна сутність M:N (Dog ↔ Feature) з власними полями AssignedAt і Notes; складений ключ (DogId, FeatureId).</summary>
public class DogFeatureEntity
{
    private DogFeatureEntity() { }

    public DogFeatureEntity(Guid dogId, Guid featureId, string? notes = null, DateTime? assignedAt = null)
    {
        if (dogId == Guid.Empty || featureId == Guid.Empty)
            throw new DomainException("DogId and FeatureId are required.");

        DogId = dogId;
        FeatureId = featureId;
        Notes = notes;
        AssignedAt = assignedAt ?? DateTime.UtcNow;
    }

    public Guid DogId { get; private set; }
    public Guid FeatureId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public string? Notes { get; private set; }

    public DogEntity Dog { get; private set; } = null!;
    public FeatureEntity Feature { get; private set; } = null!;
}
