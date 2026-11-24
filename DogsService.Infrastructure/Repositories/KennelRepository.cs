using DogsService.Domain.Entities;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.DbContext;

namespace DogsService.Infrastructure.Repositories;

public class KennelRepository(AppDbContext context) : EfRepository<KennelEntity>(context), IKennelRepository
{
}
