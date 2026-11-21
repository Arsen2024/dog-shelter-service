using DogsService.Domain.Entities;
using DogsService.Domain.Interfaces;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class KennelRepository(AppDbContext _dbContext) : IKennelRepository
{
    public IQueryable<KennelEntity> Query() => _dbContext.Kennels.AsQueryable();
}
