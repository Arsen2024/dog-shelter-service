using HealthService.Domain.Enums;

namespace HealthService.API.Contracts;

public record UpdateVisitRequest(
    long Version,
    string? Diagnosis,
    List<string>? Symptoms,
    List<string>? Recommendations,
    string? OperationNotes,
    decimal? CostAmount,
    string? CostCurrency);

public record AddAllergyRequest(string Substance, AllergySeverity Severity, DateTime? NotedAt);
public record AddNoteRequest(string Text);
public record RenameDogRequest(string Name);
