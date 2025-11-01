using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;


namespace DogsService.Application.Queries.Dog
{
    public class GetDogsQueryHandler(
        IMapper mapper,
        IUnitOfWork unitOfWork
        ) : IRequestHandler<GetDogsQuery, List<DogDto>>
    {
        public async Task<List<DogDto>> Handle(GetDogsQuery request, CancellationToken cancellationToken)
        {
            var query = unitOfWork.Dogs.Query();

            if (!string.IsNullOrEmpty(request.Attribute))
            {
                var order = request.Order?.ToLower() == "desc" ? "descending" : "ascending";
                query = query.OrderBy($"{request.Attribute} {order}");
            }

            int skip = (request.PageNumber - 1) * request.PageSize;
            query = query.Skip(skip).Take(request.PageSize);

            var dogs = query is IAsyncEnumerable<DogEntity> 
                ? await query.ToListAsync(cancellationToken)
                : query.ToList(); // for possible testing

            return mapper.Map<List<DogDto>>(dogs);
        }
    }
}
