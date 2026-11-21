using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Application.Extensions;
using DogsService.Domain.Entities;
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
        var query = _unitOfWork.Dogs.Query()
            .Include(d => d.Breed)
            .Include(d => d.Kennel);

        var sortProperty =
            request.Attribute is not null && DogSortFields.Map.TryGetValue(request.Attribute, out var mapped)
                ? mapped
                : "Name";

        var direction = string.Equals(request.Order, "desc", StringComparison.OrdinalIgnoreCase)
            ? "descending"
            : "ascending";

        // Id як другий ключ — стабільна пагінація
        IQueryable<DogEntity> ordered = query.OrderBy($"{sortProperty} {direction}, Id");

        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip = (Math.Max(request.PageNumber, 1) - 1) * pageSize;

        var dogs = await ordered.Skip(skip).Take(pageSize).ToListSafeAsync(cancellationToken);

        return _mapper.Map<List<DogDto>>(dogs);
    }
}
