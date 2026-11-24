using System.ComponentModel.DataAnnotations;

namespace DogsService.Application.DTOs;

// ------------------------- відповіді -------------------------

public class DogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int TailLength { get; set; }
    public decimal Weight { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid BreedId { get; set; }
    public string? BreedName { get; set; }
    public Guid? KennelId { get; set; }
    public string? KennelName { get; set; }
}

public class DogProfileDto
{
    public DateTime? BirthDate { get; set; }
    public string? Gender { get; set; }
    public string? Description { get; set; }
    public string? Temperament { get; set; }
}

public class DogFeatureDto
{
    public Guid FeatureId { get; set; }
    public string? FeatureName { get; set; }
    public string? Description { get; set; }
    public DateTime AssignedAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Картка собаки (Eager Loading: порода, вольєр, профіль, характеристики — одним запитом).</summary>
public class DogDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int TailLength { get; set; }
    public decimal Weight { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid BreedId { get; set; }
    public string? BreedName { get; set; }
    public Guid? KennelId { get; set; }
    public string? KennelName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DogProfileDto? Profile { get; set; }
    public List<DogFeatureDto> Features { get; set; } = [];
}

// ------------------------- запити -------------------------

/// <summary>Пагінація, фільтрація та сортування списку собак.</summary>
public class DogListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public Guid? BreedId { get; set; }

    public Guid? KennelId { get; set; }

    [RegularExpression(Patterns.DogStatus, ErrorMessage = "Status must be one of: Available, Reserved, Adopted, UnderTreatment.")]
    public string? Status { get; set; }

    /// <summary>Пошук за частиною імені.</summary>
    [StringLength(100)]
    public string? Search { get; set; }

    [RegularExpression(Patterns.SortBy, ErrorMessage = "SortBy must be one of: name, color, tailLength, weight, createdAt.")]
    public string? SortBy { get; set; }

    [RegularExpression(Patterns.SortDir, ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
    public string? SortDir { get; set; }
}

// ------------------------- команди -------------------------

public class CreateDogProfileDto
{
    public DateTime? BirthDate { get; set; }

    [RegularExpression(Patterns.Gender, ErrorMessage = "Gender must be one of: Male, Female, Unknown.")]
    public string? Gender { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Temperament { get; set; }
}

public class CreateDogDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Кольори (кількість і довжину перевіряє FluentValidation).</summary>
    [Required]
    public List<string> Colors { get; set; } = [];

    [Range(0, 1000)]
    public int TailLength { get; set; }

    [Range(0.01, 9999.99)]
    public decimal Weight { get; set; }

    [NotEmptyGuid]
    public Guid BreedId { get; set; }

    public Guid? KennelId { get; set; }

    public CreateDogProfileDto? Profile { get; set; }
}

public class ChangeWeightDto
{
    [Range(0.01, 9999.99)]
    public decimal Weight { get; set; }
}

public class ChangeStatusDto
{
    [Required, RegularExpression(Patterns.DogStatus, ErrorMessage = "Status must be one of: Available, Reserved, Adopted, UnderTreatment.")]
    public string Status { get; set; } = string.Empty;
}

public class AssignKennelDto
{
    [NotEmptyGuid]
    public Guid KennelId { get; set; }
}

public class AddFeatureDto
{
    [NotEmptyGuid]
    public Guid FeatureId { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}
