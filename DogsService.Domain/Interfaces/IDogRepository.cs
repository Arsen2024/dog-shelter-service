using DogsService.Domain.Entities;

namespace DogsService.Domain.Interfaces;

public interface IDogRepository
{
    IQueryable<DogEntity> Query();
    Task CreateAsync(DogEntity entity, CancellationToken cancellationToken);
    void Delete(DogEntity entity);
}