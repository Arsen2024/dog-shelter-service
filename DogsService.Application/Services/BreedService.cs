using AutoMapper;
using DogsService.Application.DTOs;
using DogsService.Domain.Exceptions;
using DogsService.Infrastructure.Abstractions;

namespace DogsService.Application.Services;

public class BreedService(IUnitOfWork uow, IMapper mapper) : IBreedService
{
    public async Task<List<BreedDto>> GetAllAsync(CancellationToken ct)
    {
        var breeds = await uow.Breeds.GetAllAsync(ct);
        return mapper.Map<List<BreedDto>>(breeds.OrderBy(b => b.Name));
    }

    public async Task<BreedWithDogsDto> GetByIdAsync(Guid id, bool includeDogs, CancellationToken ct)
    {
        // includeDogs = true → Explicit Loading: порода (запит №1) + собаки (запит №2)
        // includeDogs = false → лише порода (один запит), колекція Dogs не завантажується
        var breed = (includeDogs
                        ? await uow.Breeds.GetWithDogsAsync(id, ct)
                        : await uow.Breeds.GetByIdAsync(id, ct))
                    ?? throw new EntityNotFoundException("Breed", id);

        return mapper.Map<BreedWithDogsDto>(breed);
    }
}
