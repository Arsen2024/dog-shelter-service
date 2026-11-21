using DogsService.Application.Exceptions;
using DogsService.Application.Extensions;
using DogsService.Domain.Entities;
using DogsService.Domain.Enums;
using DogsService.Domain.Interfaces;
using MediatR;

namespace DogsService.Application.Commands.Dog.Create;

public class CreateDogCommandHandler(
    IUnitOfWork _unitOfWork
    ) : IRequestHandler<CreateDogCommand, Unit>
{
    public async Task<Unit> Handle(CreateDogCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        var duplicate = await _unitOfWork.Dogs.Query()
            .FirstOrDefaultSafeAsync(d => d.Name == name, cancellationToken);

        if (duplicate != null)
            throw new DuplicateDogNameException(name);

        var breed = await _unitOfWork.Breeds.Query()
            .FirstOrDefaultSafeAsync(b => b.Id == request.BreedId, cancellationToken);

        if (breed is null)
            throw new NotFoundException("Breed", request.BreedId);

        if (request.KennelId is { } kennelId)
        {
            var kennel = await _unitOfWork.Kennels.Query()
                .FirstOrDefaultSafeAsync(k => k.Id == kennelId, cancellationToken);

            if (kennel is null)
                throw new NotFoundException("Kennel", kennelId);
        }

        var dog = new DogEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Color = string.Join(" & ", request.Colors.Select(c => c.Trim().ToLower())),
            TailLength = request.TailLength,
            Weight = request.Weight,
            BreedId = request.BreedId,
            KennelId = request.KennelId,
            Status = DogStatus.Available
        };

        await _unitOfWork.Dogs.CreateAsync(dog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
