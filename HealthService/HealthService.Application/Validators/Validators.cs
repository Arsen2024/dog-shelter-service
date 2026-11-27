using FluentValidation;
using HealthService.Application.Commands;
using HealthService.Application.Queries;
using HealthService.Domain.Enums;
using HealthService.Domain.Interfaces;

namespace HealthService.Application.Validators;

// Валідатори виконує ValidationBehavior ДО виклику обробника. Правила сутностей (діапазони VO,
// ліміти довжини) додатково перевіряє сама сутність — цього достатньо, щоб не дублювати їх тут усі.

public class CreateVisitCommandValidator : AbstractValidator<CreateVisitCommand>
{
    public CreateVisitCommandValidator()
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.VisitDate).NotEmpty();
        RuleFor(x => x.VisitType).IsInEnum();

        RuleFor(x => x.TriageLevel).InclusiveBetween(1, 5).When(x => x.TriageLevel.HasValue)
            .WithMessage("TriageLevel must be between 1 and 5.");
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.TemperatureC).InclusiveBetween(30, 45).When(x => x.TemperatureC.HasValue);

        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0).When(x => x.CostAmount.HasValue);
        RuleFor(x => x.CostCurrency).Length(3).When(x => !string.IsNullOrEmpty(x.CostCurrency));
        RuleFor(x => x.VeterinarianEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.VeterinarianEmail));

        // правила залежно від типу візиту (гнучка схема)
        RuleFor(x => x.Anesthesia).NotNull()
            .When(x => x.VisitType == VisitType.Surgery)
            .WithMessage("Anesthesia is required for a Surgery visit.");
        RuleFor(x => x.TriageLevel).NotNull()
            .When(x => x.VisitType == VisitType.Emergency)
            .WithMessage("TriageLevel (1-5) is required for an Emergency visit.");
        RuleFor(x => x.RelatedVisitId).NotEmpty()
            .When(x => x.VisitType == VisitType.FollowUp)
            .WithMessage("RelatedVisitId is required for a FollowUp visit.");
        RuleFor(x => x.RelatedVisitId).MustBeObjectId()
            .When(x => !string.IsNullOrEmpty(x.RelatedVisitId));
    }
}

public class UpdateVisitCommandValidator : AbstractValidator<UpdateVisitCommand>
{
    public UpdateVisitCommandValidator()
    {
        RuleFor(x => x.Id).MustBeObjectId();
        RuleFor(x => x.Version).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostAmount).GreaterThanOrEqualTo(0).When(x => x.CostAmount.HasValue);
        RuleFor(x => x.CostCurrency).Length(3).When(x => !string.IsNullOrEmpty(x.CostCurrency));
    }
}

public class DeleteVisitCommandValidator : AbstractValidator<DeleteVisitCommand>
{
    public DeleteVisitCommandValidator() => RuleFor(x => x.Id).MustBeObjectId();
}

/// <summary>
/// АСИНХРОННЕ правило з запитом до MongoDB: картки для цієї собаки ще не існує
/// (аналог «користувач ще не залишав відгук на товар»). Додатково це гарантує унікальний індекс { dogId }.
/// </summary>
public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator(IMedicalRecordRepository records)
    {
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.DogName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BloodType).MaximumLength(50);

        RuleFor(x => x.DogId)
            .MustAsync(async (dogId, ct) => !await records.ExistsByDogIdAsync(dogId, ct))
            .When(x => x.DogId != Guid.Empty)
            .WithMessage("A medical record for this dog already exists.");
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
            .WithMessage("NextDueAt must be later than VaccinatedAt.");
        RuleFor(x => x.VeterinarianEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.VeterinarianEmail));
        RuleFor(x => x.Vaccines).NotEmpty().WithMessage("At least one vaccine is required.");
        RuleForEach(x => x.Vaccines).ChildRules(v =>
        {
            v.RuleFor(i => i.Name).NotEmpty().MaximumLength(100);
            v.RuleFor(i => i.DoseMl).GreaterThan(0).When(i => i.DoseMl.HasValue);
        });
    }
}

public class GetVisitByIdQueryValidator : AbstractValidator<GetVisitByIdQuery>
{
    public GetVisitByIdQueryValidator() => RuleFor(x => x.Id).MustBeObjectId();
}

public class GetVisitsListQueryValidator : AbstractValidator<GetVisitsListQuery>
{
    public GetVisitsListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

public class GetMedicalRecordByDogIdQueryValidator : AbstractValidator<GetMedicalRecordByDogIdQuery>
{
    public GetMedicalRecordByDogIdQueryValidator() => RuleFor(x => x.DogId).NotEmpty();
}

public class GetVaccinationsByDogIdQueryValidator : AbstractValidator<GetVaccinationsByDogIdQuery>
{
    public GetVaccinationsByDogIdQueryValidator() => RuleFor(x => x.DogId).NotEmpty();
}
