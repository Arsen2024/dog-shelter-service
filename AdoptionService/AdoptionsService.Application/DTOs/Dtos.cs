namespace AdoptionsService.Application.DTOs;

public record AdopterDto(Guid Id, string FullName, string Email, string? Phone, string? Address, DateTime CreatedAt);

public record ApplicationListItemDto(Guid Id, string Status, DateTime SubmittedAt, Guid AdopterId, string AdopterName);

public record ApplicationDogDto(Guid DogId, string DogNameAtSubmission, string CurrentName, string CurrentStatus, int Priority);

public record ApplicationRequirementDto(string Code, string Title, bool IsMandatory, bool IsFulfilled, DateTime? CheckedAt, string? CheckedBy);

public record StatusHistoryDto(string? OldStatus, string NewStatus, DateTime ChangedAt, string? ChangedBy, string? Comment);

public record AgreementDto(string AgreementNumber, DateTime SignedAt);

public record ApplicationDetailsDto(
    Guid Id,
    string Status,
    DateTime SubmittedAt,
    string? DecisionNote,
    Guid AdopterId,
    string AdopterName,
    string AdopterEmail,
    AgreementDto? Agreement,
    List<ApplicationDogDto> Dogs,
    List<ApplicationRequirementDto> Requirements,
    List<StatusHistoryDto> History);

public record RequirementDto(Guid Id, string Code, string Title, string? Description, bool IsMandatory);

public record DogDto(Guid Id, string Name, string Status, DateTime SyncedAt);
