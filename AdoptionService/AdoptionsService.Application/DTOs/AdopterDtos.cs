using System.ComponentModel.DataAnnotations;

namespace AdoptionsService.Application.DTOs;

public class AdopterDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Base64 від rowversion — повертається клієнту й має бути передана назад у PUT.</summary>
    public string RowVersion { get; set; } = string.Empty;
}

public class CreateAdopterDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(400)]
    public string? Address { get; set; }
}

public class UpdateAdopterDto
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(400)]
    public string? Address { get; set; }

    [Required]
    public string RowVersion { get; set; } = string.Empty;
}

public class PageQuery
{
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}
