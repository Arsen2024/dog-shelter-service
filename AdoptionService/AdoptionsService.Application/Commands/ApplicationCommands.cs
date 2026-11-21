using AdoptionsService.Domain.Exceptions;
using AdoptionsService.Domain.Interfaces;
using MediatR;

namespace AdoptionsService.Application.Commands;

// ---------- usp_CreateApplication ----------
public record CreateApplicationCommand(Guid AdopterId, Guid DogId) : IRequest<Guid>;

public class CreateApplicationCommandHandler(
    IApplicationRepository _applications,
    ICurrentUser _currentUser
    ) : IRequestHandler<CreateApplicationCommand, Guid>
{
    public Task<Guid> Handle(CreateApplicationCommand request, CancellationToken cancellationToken) =>
        _applications.CreateAsync(request.AdopterId, request.DogId, _currentUser.UserName, cancellationToken);
}

// ---------- usp_AddDogToApplication ----------
public record AddDogToApplicationCommand(Guid ApplicationId, Guid DogId, int Priority = 1) : IRequest<Unit>;

public class AddDogToApplicationCommandHandler(
    IApplicationRepository _applications
    ) : IRequestHandler<AddDogToApplicationCommand, Unit>
{
    public async Task<Unit> Handle(AddDogToApplicationCommand request, CancellationToken cancellationToken)
    {
        await _applications.AddDogAsync(request.ApplicationId, request.DogId, request.Priority, cancellationToken);
        return Unit.Value;
    }
}

// ---------- usp_ChangeApplicationStatus (явна транзакція в БД) ----------
public record ChangeApplicationStatusCommand(Guid ApplicationId, string NewStatus, string? Comment) : IRequest<Unit>;

public class ChangeApplicationStatusCommandHandler(
    IApplicationRepository _applications,
    ICurrentUser _currentUser
    ) : IRequestHandler<ChangeApplicationStatusCommand, Unit>
{
    public async Task<Unit> Handle(ChangeApplicationStatusCommand request, CancellationToken cancellationToken)
    {
        await _applications.ChangeStatusAsync(
            request.ApplicationId, request.NewStatus, _currentUser.UserName, request.Comment, cancellationToken);
        return Unit.Value;
    }
}

// ---------- usp_SignAgreement ----------
public record SignAgreementCommand(
    Guid ApplicationId, Guid DogId, string AgreementNumber, string SignedBy, string? Terms) : IRequest<Unit>;

public class SignAgreementCommandHandler(
    IApplicationRepository _applications
    ) : IRequestHandler<SignAgreementCommand, Unit>
{
    public async Task<Unit> Handle(SignAgreementCommand request, CancellationToken cancellationToken)
    {
        await _applications.SignAgreementAsync(
            request.ApplicationId, request.DogId, request.AgreementNumber.Trim(),
            request.SignedBy.Trim(), request.Terms, cancellationToken);
        return Unit.Value;
    }
}

// ---------- відмітка виконання вимоги (M:N ApplicationRequirements) ----------
public record FulfillRequirementCommand(Guid ApplicationId, Guid RequirementId, string? Notes) : IRequest<Unit>;

public class FulfillRequirementCommandHandler(
    IApplicationRepository _applications,
    ICurrentUser _currentUser
    ) : IRequestHandler<FulfillRequirementCommand, Unit>
{
    public async Task<Unit> Handle(FulfillRequirementCommand request, CancellationToken cancellationToken)
    {
        var updated = await _applications.FulfillRequirementAsync(
            request.ApplicationId, request.RequirementId, _currentUser.UserName, request.Notes, cancellationToken);

        if (!updated)
            throw new NotFoundException(
                $"Requirement '{request.RequirementId}' is not attached to application '{request.ApplicationId}'.");

        return Unit.Value;
    }
}
