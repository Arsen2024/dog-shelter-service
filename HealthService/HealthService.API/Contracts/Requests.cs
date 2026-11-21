using HealthService.Domain.Enums;

namespace HealthService.API.Contracts;

public record AddAllergyRequest(string Substance, AllergySeverity Severity, DateTime? NotedAt);
public record AddNoteRequest(string Text);
public record RenameDogRequest(string Name);
