using DogsService.Application.Commands.Dog.Create;
using FluentValidation;

namespace DogsService.Application.Validators;

public class CreateDogCommandValidator : AbstractValidator<CreateDogCommand>
{
    public CreateDogCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Must(name => name == null || name.Trim().Length > 0)
            .WithMessage("Name cannot be only spaces")
            .MaximumLength(100);

        RuleFor(x => x.Colors)
            .NotEmpty().WithMessage("At least one color is required");

        RuleForEach(x => x.Colors)
            .NotEmpty().WithMessage("Color cannot be empty");

        RuleFor(x => x.TailLength)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tail length cannot be negative");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than zero")
            .LessThan(10000).WithMessage("Weight is too large");

        RuleFor(x => x.BreedId)
            .NotEmpty().WithMessage("BreedId is required");
    }
}
