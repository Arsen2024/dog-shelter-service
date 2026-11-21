using MediatR;

namespace DogsService.Application.Commands.Dog.Delete;
public record DeleteDogCommand(Guid Id) : IRequest<Unit>;