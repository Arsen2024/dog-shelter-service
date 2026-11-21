using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class BreedRepository(AppDbContext _dbContext) : IBreedRepository
{
    public IQueryable<BreedEntity> Query() => _dbContext.Breeds.AsQueryable();
}
