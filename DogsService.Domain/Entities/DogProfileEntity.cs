namespace DogsService.Domain.Entities;

/// <summary>
/// 1:1 з Dog. DogId одночасно є PK і FK (залежна сутність не має власного Id).
/// </summary>
public class DogProfileEntity
{
    public Guid DogId { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Gender { get; set; }

    public string? Description { get; set; }

    public string? Temperament { get; set; }

    // Navigation property
    public DogEntity Dog { get; set; } = null!;
}
