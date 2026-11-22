using System.ComponentModel.DataAnnotations;

namespace AdoptionsService.Application.DTOs;

// ------------------------- відповіді -------------------------

public class ApplicationListItemDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public Guid AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
}

public class ApplicationDogDto
{
    public Guid DogId { get; set; }
    public string DogName { get; set; } = string.Empty;   // знімок на момент заявки
    public int Priority { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>Заявка з собаками (результат multi-mapping запиту).</summary>
public class ApplicationDto
{
    public Guid Id { get; set; }
    public Guid AdopterId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? DecisionNote { get; set; }
    public string RowVersion { get; set; } = string.Empty;
    public List<ApplicationDogDto> Dogs { get; set; } = [];
}

public class ApplicationDetailsDogDto
{
    public Guid DogId { get; set; }
    public string DogNameAtSubmission { get; set; } = string.Empty;
    public string CurrentName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class ApplicationRequirementDto
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsFulfilled { get; set; }
    public DateTime? CheckedAt { get; set; }
    public string? CheckedBy { get; set; }
}

public class StatusHistoryDto
{
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comment { get; set; }
}

/// <summary>Повні деталі заявки (результат процедури usp_GetApplicationDetails).</summary>
public class ApplicationDetailsDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? DecisionNote { get; set; }
    public Guid AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
    public string AdopterEmail { get; set; } = string.Empty;
    public string? AgreementNumber { get; set; }
    public DateTime? AgreementSignedAt { get; set; }
    public List<ApplicationDetailsDogDto> Dogs { get; set; } = [];
    public List<ApplicationRequirementDto> Requirements { get; set; } = [];
    public List<StatusHistoryDto> History { get; set; } = [];
}

public class RequirementDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
}

public class DogDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}

// ------------------------- запити (DataAnnotations) -------------------------

public class ApplicationListQuery
{
    [RegularExpression(StatusPatterns.ApplicationStatus,
        ErrorMessage = "Status must be one of: Submitted, UnderReview, Approved, Completed, Rejected, Cancelled.")]
    public string? Status { get; set; }

    public Guid? AdopterId { get; set; }

    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
}

public class CreateApplicationDto
{
    [NotEmptyGuid]
    public Guid AdopterId { get; set; }

    [NotEmptyGuid]
    public Guid DogId { get; set; }
}

public class AddDogDto
{
    [NotEmptyGuid]
    public Guid DogId { get; set; }

    [Range(1, 100)]
    public int Priority { get; set; } = 1;
}

public class ChangeStatusDto
{
    [Required, RegularExpression(StatusPatterns.ApplicationStatus,
        ErrorMessage = "NewStatus must be one of: Submitted, UnderReview, Approved, Completed, Rejected, Cancelled.")]
    public string NewStatus { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Comment { get; set; }
}

public class SignAgreementDto
{
    [NotEmptyGuid]
    public Guid DogId { get; set; }

    [Required, StringLength(50, MinimumLength = 1)]
    public string AgreementNumber { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 1)]
    public string SignedBy { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Terms { get; set; }
}

public class FulfillRequirementDto
{
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class SyncDogDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, RegularExpression(StatusPatterns.DogStatus,
        ErrorMessage = "Status must be one of: Available, Reserved, Adopted, UnderTreatment.")]
    public string Status { get; set; } = string.Empty;
}
