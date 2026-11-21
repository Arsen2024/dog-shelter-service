using AdoptionsService.Domain.Interfaces;
using MediatR;

namespace AdoptionsService.Application.Commands;

public record CreateAdopterCommand(string FullName, string Email, string? Phone, string? Address) : IRequest<Guid>;

public class CreateAdopterCommandHandler(
    IAdopterRepository _adopters,
    ICurrentUser _currentUser
    ) : IRequestHandler<CreateAdopterCommand, Guid>
{
    public Task<Guid> Handle(CreateAdopterCommand request, CancellationToken cancellationToken) =>
        _adopters.CreateAsync(
            request.FullName.Trim(), request.Email.Trim(), request.Phone, request.Address,
            _currentUser.UserName, cancellationToken);
}
