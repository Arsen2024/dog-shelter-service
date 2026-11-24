using DogsService.Domain.Exceptions;

namespace DogsService.Domain.Entities;

public class FeatureEntity
{
    private readonly List<DogFeatureEntity> _dogFeatures = new();

    private FeatureEntity() { }

    public FeatureEntity(string name, string? description = null, Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Feature name is required.");

        Id = id ?? Guid.NewGuid();
        Name = name.Trim();
        Description = description;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IReadOnlyCollection<DogFeatureEntity> DogFeatures => _dogFeatures;
}
