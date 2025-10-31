using DogsService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Domain.Interfaces
{
    public interface IDogRepository
    {
        IQueryable<DogEntity> Query();
        Task CreateAsync(DogEntity entity, CancellationToken cancellationToken);
    }
}
