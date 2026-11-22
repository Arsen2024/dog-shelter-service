using System.ComponentModel.DataAnnotations;

namespace AdoptionsService.Application.DTOs;

/// <summary>Guid не може бути порожнім (00000000-...).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute() : base("The {0} field must be a non-empty GUID.") { }

    public override bool IsValid(object? value) => value is Guid g && g != Guid.Empty;
}

public static class StatusPatterns
{
    public const string ApplicationStatus = "^(Submitted|UnderReview|Approved|Completed|Rejected|Cancelled)$";
    public const string DogStatus = "^(Available|Reserved|Adopted|UnderTreatment)$";
}
