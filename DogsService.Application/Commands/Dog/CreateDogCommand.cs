using MediatR;

namespace DogsService.Application.Commands.Dog;

public record CreateDogCommand(string Name, List<string> Colors, int TailLength, int Weight) : IRequest<Unit>;