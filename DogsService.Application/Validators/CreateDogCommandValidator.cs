using DogsService.Application.Commands.Dog.Create;
using FluentValidation;

namespace DogsService.Application.Validators;

public class CreateDogCommandValidator : AbstractValidator<CreateDogCommand>
{
    public CreateDogCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Name is required")
            .Must(name => name == null || name.Trim().Length > 0)
            .WithMessage("Name cannot be only spaces");

        RuleFor(x => x.Colors)
            .NotEmpty().WithMessage("At least one color is required");

        RuleFor(x => x.TailLength)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tail length cannot be negative");

        RuleFor(x => x.Weight)
            .GreaterThan(0)
            .WithMessage("Weight must be greater than zero");
    }
}