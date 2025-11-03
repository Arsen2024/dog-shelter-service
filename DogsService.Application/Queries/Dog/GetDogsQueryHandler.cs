using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Application.Extensions;
using DogsService.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace DogsService.Application.Queries.Dog;

public class GetDogsQueryHandler(
    IMapper _mapper,
    IUnitOfWork _unitOfWork
    ) : IRequestHandler<GetDogsQuery, List<DogDto>>
{
    public async Task<List<DogDto>> Handle(GetDogsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Dogs.Query();

        if (!string.IsNullOrEmpty(request.Attribute))
        {
            var order = request.Order?.ToLower() == "desc" ? "descending" : "ascending";
            query = query.OrderBy($"{request.Attribute} {order}");
        }

        int skip = (request.PageNumber - 1) * request.PageSize;
        query = query.Skip(skip).Take(request.PageSize);

        var dogs = await query.ToListSafeAsync(cancellationToken);

        return _mapper.Map<List<DogDto>>(dogs);
    }
}