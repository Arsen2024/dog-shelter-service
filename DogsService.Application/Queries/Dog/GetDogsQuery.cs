using DogsService.Application.DTOs.Dog;
using MediatR;

namespace DogsService.Application.Queries.Dog;

public record GetDogsQuery(string? Attribute, string? Order, int PageNumber = 1, int PageSize = 10) : IRequest<List<DogDto>>;