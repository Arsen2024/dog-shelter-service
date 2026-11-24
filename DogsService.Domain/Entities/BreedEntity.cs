using DogsService.Domain.Exceptions;

namespace DogsService.Domain.Entities;

public class BreedEntity
{
    private readonly List<DogEntity> _dogs = new();

    private BreedEntity() { }

    public BreedEntity(string name, string? description = null, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Rename(name);
        Description = description;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IReadOnlyCollection<DogEntity> Dogs => _dogs;

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Breed name is required.");

        Name = name.Trim();
    }

    public void ChangeDescription(string? description) => Description = description;
}
