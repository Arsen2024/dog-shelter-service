using DogsService.Domain.Exceptions;

namespace DogsService.Domain.Entities;

public class KennelEntity
{
    private readonly List<DogEntity> _dogs = new();

    private KennelEntity() { }

    public KennelEntity(string name, string location, int capacity, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Name = RequireText(name, "Kennel name");
        Location = RequireText(location, "Kennel location");
        Capacity = ValidateCapacity(capacity);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    public IReadOnlyCollection<DogEntity> Dogs => _dogs;

    public void ChangeCapacity(int capacity) => Capacity = ValidateCapacity(capacity);

    private static string RequireText(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{field} is required.");

        return value.Trim();
    }

    private static int ValidateCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new DomainException("Kennel capacity must be greater than zero.");

        return capacity;
    }
}
