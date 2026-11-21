using AdoptionsService.Domain.Interfaces;
using MediatR;

namespace AdoptionsService.Application.Commands;

/// <summary>
/// Оновлення локальної копії собаки. У Лаб. 08 викликатиметься обробником події DogUpdated із DogsService;
/// зараз доступно через внутрішній endpoint для імітації синхронізації.
/// </summary>
public record SyncDogCommand(Guid Id, string Name, string Status) : IRequest<Unit>;

public class SyncDogCommandHandler(IDogRepository _dogs) : IRequestHandler<SyncDogCommand, Unit>
{
    public async Task<Unit> Handle(SyncDogCommand request, CancellationToken cancellationToken)
    {
        await _dogs.UpsertAsync(request.Id, request.Name.Trim(), request.Status, cancellationToken);
        return Unit.Value;
    }
}
