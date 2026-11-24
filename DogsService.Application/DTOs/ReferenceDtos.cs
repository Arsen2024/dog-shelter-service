namespace DogsService.Application.DTOs;

public class BreedDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class BreedDogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>Порода з собаками (Explicit Loading: собаки завантажуються окремим запитом).</summary>
public class BreedWithDogsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<BreedDogDto> Dogs { get; set; } = [];
}

public class KennelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Occupied { get; set; }
}

public class FeatureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class FeatureAssignmentDto
{
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;
    public string BreedName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public string? Notes { get; set; }
}
