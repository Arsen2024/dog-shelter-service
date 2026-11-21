namespace AdoptionsService.Domain.Models;

// Read-моделі для запитів (відображення результатів збережених процедур / JOIN-ів)

public class ApplicationListItemModel
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public Guid AdopterId { get; set; }
    public string AdopterName { get; set; } = string.Empty;
}

public class ApplicationHeaderModel
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public string? DecisionNote { get; set; }
    public Guid AdopterId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AgreementNumber { get; set; }
    public DateTime? SignedAt { get; set; }
}

public class ApplicationDogModel
{
    public Guid DogId { get; set; }
    public string DogNameAtSubmission { get; set; } = string.Empty;
    public string CurrentName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public class ApplicationRequirementModel
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsMandatory { get; set; }
    public bool IsFulfilled { get; set; }
    public DateTime? CheckedAt { get; set; }
    public string? CheckedBy { get; set; }
}

public class StatusHistoryModel
{
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comment { get; set; }
}

public class ApplicationDetailsModel
{
    public ApplicationHeaderModel Header { get; set; } = null!;
    public List<ApplicationDogModel> Dogs { get; set; } = [];
    public List<ApplicationRequirementModel> Requirements { get; set; } = [];
    public List<StatusHistoryModel> History { get; set; } = [];
}
