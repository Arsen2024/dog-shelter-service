using DogsService.Domain.Entities;

namespace DogsService.Domain.Interfaces;

public interface IBreedRepository
{
    IQueryable<BreedEntity> Query();
}
