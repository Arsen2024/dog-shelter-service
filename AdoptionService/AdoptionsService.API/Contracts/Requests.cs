namespace AdoptionsService.API.Contracts;

public record ChangeStatusRequest(string NewStatus, string? Comment);
public record AddDogRequest(Guid DogId, int Priority = 1);
public record SignAgreementRequest(Guid DogId, string AgreementNumber, string SignedBy, string? Terms);
public record FulfillRequirementRequest(string? Notes);
public record SyncDogRequest(string Name, string Status);
