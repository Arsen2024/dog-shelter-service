namespace DogsService.Domain.Models;

/// <summary>Проєкція зв'язку «собака ↔ характеристика» разом із власними полями проміжної сутності.</summary>
public class FeatureAssignmentModel
{
    public FeatureAssignmentModel(Guid dogId, string dogName, string breedName, DateTime assignedAt, string? notes)
    {
        DogId = dogId;
        DogName = dogName;
        BreedName = breedName;
        AssignedAt = assignedAt;
        Notes = notes;
    }

    public Guid DogId { get; }
    public string DogName { get; }
    public string BreedName { get; }
    public DateTime AssignedAt { get; }
    public string? Notes { get; }
}
