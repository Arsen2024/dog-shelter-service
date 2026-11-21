using DogsService.Domain.Entities;

namespace DogsService.Domain.Interfaces;

public interface IKennelRepository
{
    IQueryable<KennelEntity> Query();
}
