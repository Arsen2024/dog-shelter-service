using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class DogRepository(
    AppDbContext _dbContext
    ) : IDogRepository
{
    public IQueryable<DogEntity> Query()
        => _dbContext.Dogs.AsQueryable();

    public async Task CreateAsync(DogEntity entity, CancellationToken cancellationToken)
        => await _dbContext.Dogs.AddAsync(entity, cancellationToken);
}