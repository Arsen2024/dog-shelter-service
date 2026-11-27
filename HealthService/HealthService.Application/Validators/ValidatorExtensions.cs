using FluentValidation;
using MongoDB.Bson;

namespace HealthService.Application.Validators;

public static class ValidatorExtensions
{
    /// <summary>Рядок має бути коректним ObjectId (24 hex-символи).</summary>
    public static IRuleBuilderOptions<T, string?> MustBeObjectId<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(value => value is not null && ObjectId.TryParse(value, out _))
            .WithMessage("'{PropertyName}' must be a valid ObjectId (24 hex characters).");
}
