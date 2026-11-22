namespace AdoptionsService.Domain.Entities;

public class AdopterEntity
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class AdoptionApplicationEntity
{
    public Guid Id { get; set; }
    public Guid AdopterId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? DecisionNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public byte[] RowVersion { get; set; } = [];

    /// <summary>Заповнюється multi-mapping запитом (заявка + собаки одним SELECT).</summary>
    public List<ApplicationDogEntity> Dogs { get; set; } = [];
}

public class ApplicationDogEntity
{
    public Guid ApplicationId { get; set; }
    public Guid DogId { get; set; }
    /// <summary>Знімок імені на момент подання заявки — не оновлюється.</summary>
    public string DogName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTime AddedAt { get; set; }
}

public class ApplicationStatusHistoryEntity
{
    public long Id { get; set; }
    public Guid ApplicationId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comment { get; set; }
}

/// <summary>1:1 із заявкою: ApplicationId одночасно PK і FK.</summary>
public class AdoptionAgreementEntity
{
    public Guid ApplicationId { get; set; }
    public Guid DogId { get; set; }
    public string AgreementNumber { get; set; } = string.Empty;
    public DateTime SignedAt { get; set; }
    public string SignedBy { get; set; } = string.Empty;
    public string? Terms { get; set; }
}

public class RequirementEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; }
}

public class ApplicationRequirementEntity
{
    public Guid ApplicationId { get; set; }
    public Guid RequirementId { get; set; }
    public bool IsFulfilled { get; set; }
    public DateTime? CheckedAt { get; set; }
    public string? CheckedBy { get; set; }
    public string? Notes { get; set; }
}

/// <summary>Локальна копія довідника собак з DogsService (власник даних — Каталог).</summary>
public class DogEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SyncedAt { get; set; }
}
