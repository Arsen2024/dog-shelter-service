using DogsService.Domain.Enums;

namespace DogsService.Domain.Entities;

public class DogEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int TailLength { get; set; }

    public decimal Weight { get; set; }

    public Guid BreedId { get; set; }

    public Guid? KennelId { get; set; }

    public DogStatus Status { get; set; } = DogStatus.Available;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    // Navigation properties

    public BreedEntity Breed { get; set; } = null!;

    public KennelEntity? Kennel { get; set; }

    public DogProfileEntity? Profile { get; set; }

    public ICollection<DogFeatureEntity> DogFeatures { get; set; }
        = new List<DogFeatureEntity>();
}