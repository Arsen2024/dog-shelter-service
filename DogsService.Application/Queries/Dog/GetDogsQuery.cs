using DogsService.Application.DTOs;
using DogsService.Application.DTOs.Dog;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Application.Queries.Dog
{
    public record GetDogsQuery(string? Attribute, string? Order, int PageNumber = 1, int PageSize = 10) : IRequest<List<DogDto>>;
}
