using DogsService.Application.Queries.Dog;
using FluentValidation;

namespace DogsService.Application.Validators;

public class GetDogsQueryValidator : AbstractValidator<GetDogsQuery>
{
    public GetDogsQueryValidator()
    {
        RuleFor(x => x.Attribute)
            .Must(a => DogSortFields.Map.ContainsKey(a!))
            .When(x => !string.IsNullOrEmpty(x.Attribute))
            .WithMessage($"Attribute must be one of: {string.Join(", ", DogSortFields.Map.Keys)}");

        RuleFor(x => x.Order)
            .Must(o => o!.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    || o!.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .When(x => !string.IsNullOrEmpty(x.Order))
            .WithMessage("Order must be 'asc' or 'desc'");

        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
