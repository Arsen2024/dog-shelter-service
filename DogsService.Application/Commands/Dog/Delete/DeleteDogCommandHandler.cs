using DogsService.Application.Exceptions;
using DogsService.Application.Extensions;
using DogsService.Domain.Interfaces;
using MediatR;

namespace DogsService.Application.Commands.Dog.Delete;

public class DeleteDogCommandHandler(
    IUnitOfWork _unitOfWork
    ) : IRequestHandler<DeleteDogCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDogCommand command, CancellationToken cancellationToken)
    {
        var dog = await _unitOfWork.Dogs.Query()
            .FirstOrDefaultSafeAsync(x => x.Id == command.Id, cancellationToken);

        if (dog is null)
            throw new NotFoundException("Dog", command.Id);

        // AppDbContext перетворює Delete на soft delete (IsDeleted = true)
        _unitOfWork.Dogs.Delete(dog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
