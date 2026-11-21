using MediatR;

namespace DogsService.Application.Commands.Dog.Create;

public record CreateDogCommand(
    string Name,
    List<string> Colors,
    int TailLength,
    decimal Weight,
    Guid BreedId,
    Guid? KennelId = null) : IRequest<Unit>;
