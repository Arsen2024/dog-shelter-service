using FluentValidation;
using HealthService.Application.Commands;
using HealthService.Application.Queries;
using HealthService.Domain.Enums;

namespace HealthService.Application.Validators;

public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.DogName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BloodType).MaximumLength(50);
    }
}

public class AddAllergyCommandValidator : AbstractValidator<AddAllergyCommand>
{
    public AddAllergyCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.Substance).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Severity).IsInEnum();
    }
}

public class AddMedicalNoteCommandValidator : AbstractValidator<AddMedicalNoteCommand>
{
    public AddMedicalNoteCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.Text).NotEmpty().MaximumLength(2000);
    }
}

public class RenameDogCommandValidator : AbstractValidator<RenameDogCommand>
{
    public RenameDogCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class RecordVaccinationCommandValidator : AbstractValidator<RecordVaccinationCommand>
{
    public RecordVaccinationCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.VaccinatedAt).NotEmpty();
        RuleFor(x => x.NextDueAt)
            .GreaterThan(x => x.VaccinatedAt)
            .When(x => x.NextDueAt.HasValue)
            .WithMessage("NextDueAt must be later than VaccinatedAt");
        RuleFor(x => x.Vaccines).NotEmpty().WithMessage("At least one vaccine is required");
        RuleForEach(x => x.Vaccines).ChildRules(v =>
        {
            v.RuleFor(i => i.Name).NotEmpty().MaximumLength(100);
            v.RuleFor(i => i.DoseMl).GreaterThan(0).When(i => i.DoseMl.HasValue);
        });
    }
}

public class RecordVisitCommandValidator : AbstractValidator<RecordVisitCommand>
{
    private const string ObjectIdPattern = "^[0-9a-fA-F]{24}$";

    public RecordVisitCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.VisitDate).NotEmpty();
        RuleFor(x => x.VisitType).IsInEnum();

        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.TemperatureC).InclusiveBetween(30, 45).When(x => x.TemperatureC.HasValue);
        RuleFor(x => x.TriageLevel).InclusiveBetween(1, 5).When(x => x.TriageLevel.HasValue);

        // Правила залежно від типу візиту (гнучка схема)
        RuleFor(x => x.Anesthesia).NotNull()
            .When(x => x.VisitType == VisitType.Surgery)
            .WithMessage("Anesthesia is required for a Surgery visit");
        RuleFor(x => x.TriageLevel).NotNull()
            .When(x => x.VisitType == VisitType.Emergency)
            .WithMessage("TriageLevel (1-5) is required for an Emergency visit");
        RuleFor(x => x.RelatedVisitId).NotEmpty()
            .When(x => x.VisitType == VisitType.FollowUp)
            .WithMessage("RelatedVisitId is required for a FollowUp visit");
        RuleFor(x => x.RelatedVisitId).Matches(ObjectIdPattern)
            .When(x => !string.IsNullOrEmpty(x.RelatedVisitId))
            .WithMessage("RelatedVisitId must be a valid ObjectId");
    }
}

public class GetVisitByIdQueryValidator : AbstractValidator<GetVisitByIdQuery>
{
    public GetVisitByIdQueryValidator()
    {
        RuleFor(x => x.Id).Matches("^[0-9a-fA-F]{24}$").WithMessage("Id must be a valid ObjectId");
    }
}

public class GetVisitsByDogIdQueryValidator : AbstractValidator<GetVisitsByDogIdQuery>
{
    public GetVisitsByDogIdQueryValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
