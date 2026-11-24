using DogsService.Domain.Exceptions;

namespace DogsService.Domain.Entities;

/// <summary>1:1 з Dog. DogId одночасно є PK і FK.</summary>
public class DogProfileEntity
{
    private static readonly string[] AllowedGenders = ["Male", "Female", "Unknown"];

    private DogProfileEntity() { }

    // перший параметр — навігація, тому EF Core не вважає цей конструктор придатним для матеріалізації
    public DogProfileEntity(DogEntity dog, DateTime? birthDate, string? gender, string? description, string? temperament)
    {
        Dog = dog;
        DogId = dog.Id;
        Update(birthDate, gender, description, temperament);
    }

    public Guid DogId { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? Gender { get; private set; }
    public string? Description { get; private set; }
    public string? Temperament { get; private set; }

    public DogEntity Dog { get; private set; } = null!;

    public void Update(DateTime? birthDate, string? gender, string? description, string? temperament)
    {
        if (birthDate.HasValue && birthDate.Value.Date > DateTime.UtcNow.Date)
            throw new DomainException("Birth date cannot be in the future.");

        if (gender is not null && !AllowedGenders.Contains(gender))
            throw new DomainException($"Gender must be one of: {string.Join(", ", AllowedGenders)}.");

        BirthDate = birthDate;
        Gender = gender;
        Description = description;
        Temperament = temperament;
    }
}
