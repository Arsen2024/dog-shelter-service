using DogsService.Domain.Entities;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class FeatureRepository(AppDbContext context) : EfRepository<FeatureEntity>(context), IFeatureRepository
{
}
