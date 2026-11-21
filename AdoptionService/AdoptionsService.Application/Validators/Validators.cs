using AdoptionsService.Application.Commands;
using AdoptionsService.Application.Queries;
using AdoptionsService.Domain.Enums;
using FluentValidation;

namespace AdoptionsService.Application.Validators;

public class CreateAdopterCommandValidator : AbstractValidator<CreateAdopterCommand>
{
    public CreateAdopterCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(30);
        RuleFor(x => x.Address).MaximumLength(400);
    }
}

public class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.AdopterId).NotEmpty();
        RuleFor(x => x.DogId).NotEmpty();
    }
}

public class AddDogToApplicationCommandValidator : AbstractValidator<AddDogToApplicationCommand>
{
    public AddDogToApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.Priority).GreaterThan(0);
    }
}

public class ChangeApplicationStatusCommandValidator : AbstractValidator<ChangeApplicationStatusCommand>
{
    public ChangeApplicationStatusCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .NotEmpty()
            .Must(s => ApplicationStatuses.All.Contains(s))
            .WithMessage($"NewStatus must be one of: {string.Join(", ", ApplicationStatuses.All)}");
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}

public class SignAgreementCommandValidator : AbstractValidator<SignAgreementCommand>
{
    public SignAgreementCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.DogId).NotEmpty();
        RuleFor(x => x.AgreementNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SignedBy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Terms).MaximumLength(2000);
    }
}

public class FulfillRequirementCommandValidator : AbstractValidator<FulfillRequirementCommand>
{
    public FulfillRequirementCommandValidator()
    {
        RuleFor(x => x.ApplicationId).NotEmpty();
        RuleFor(x => x.RequirementId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

public class SyncDogCommandValidator : AbstractValidator<SyncDogCommand>
{
    public SyncDogCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status)
            .Must(s => DogStatuses.All.Contains(s))
            .WithMessage($"Status must be one of: {string.Join(", ", DogStatuses.All)}");
    }
}

public class GetAdoptersQueryValidator : AbstractValidator<GetAdoptersQuery>
{
    public GetAdoptersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public class GetApplicationsQueryValidator : AbstractValidator<GetApplicationsQuery>
{
    public GetApplicationsQueryValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => ApplicationStatuses.All.Contains(s!))
            .When(x => !string.IsNullOrEmpty(x.Status))
            .WithMessage($"Status must be one of: {string.Join(", ", ApplicationStatuses.All)}");
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
