using DogsService.Application.DTOs;
using FluentValidation;

namespace DogsService.Application.Validators;

// Прості правила (обов'язковість, довжини, діапазони) задані DataAnnotations на DTO.
// Тут — лише складні правила, яких DataAnnotations виразити не може; без дублювання.

public class CreateDogDtoValidator : AbstractValidator<CreateDogDto>
{
    public CreateDogDtoValidator()
    {
        RuleFor(x => x.Colors)
            .NotEmpty().WithMessage("At least one color is required.")
            .Must(c => c.Count <= 5).WithMessage("A dog cannot have more than 5 colors.");

        RuleForEach(x => x.Colors)
            .Must(c => !string.IsNullOrWhiteSpace(c)).WithMessage("Color cannot be empty.")
            .Must(c => c is null || c.Trim().Length <= 30).WithMessage("Color cannot be longer than 30 characters.");

        // decimal(6,2): не більше 4 цифр до коми і 2 після
        RuleFor(x => x.Weight)
            .PrecisionScale(6, 2, false).WithMessage("Weight can have at most 4 integer digits and 2 decimal places.");

        When(x => x.Profile is not null, () =>
        {
            RuleFor(x => x.Profile!.BirthDate)
                .Must(d => d is null || d.Value.Date <= DateTime.UtcNow.Date)
                .WithMessage("Birth date cannot be in the future.");
        });
    }
}

public class ChangeWeightDtoValidator : AbstractValidator<ChangeWeightDto>
{
    public ChangeWeightDtoValidator()
    {
        RuleFor(x => x.Weight)
            .PrecisionScale(6, 2, false).WithMessage("Weight can have at most 4 integer digits and 2 decimal places.");
    }
}
