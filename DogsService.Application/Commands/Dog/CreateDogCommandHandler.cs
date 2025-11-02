using DogsService.Application.Exceptions;
using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Application.Commands.Dog;

public class CreateDogCommandHandler(
    IUnitOfWork _unitOfWork
    ) : IRequestHandler<CreateDogCommand, Unit>
{
    public async Task<Unit> Handle(CreateDogCommand request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Dogs.Query();

        var dublicate = query is IAsyncEnumerable<DogEntity>
            ? await query.FirstOrDefaultAsync(d => d.Name == request.Name, cancellationToken)
            : query.FirstOrDefault(d => d.Name == request.Name); // for possible testing

        if (dublicate != null)
            throw new DuplicateDogNameException(request.Name);

        var dog = new DogEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = string.Join(" & ", request.Colors.Select(c => c.Trim().ToLower())),
            TailLength = request.TailLength,
            Weight = request.Weight,
        };

        await _unitOfWork.Dogs.CreateAsync(dog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}