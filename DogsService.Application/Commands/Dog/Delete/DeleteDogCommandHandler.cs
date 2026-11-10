using DogsService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DogsService.Application.Commands.Dog.Delete;
public class DeleteDogCommandHandler(
    IUnitOfWork _unitOfWork
    ) : IRequestHandler<DeleteDogCommand, Unit>
{
    public async Task<Unit> Handle(DeleteDogCommand command, CancellationToken cancellationToken)
    {
        var dog = await _unitOfWork.Dogs.Query().FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (dog is null)
        {
            throw new Exception($"Dog with id {command.Id} not found");
        }

        _unitOfWork.Dogs.Delete(dog);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}