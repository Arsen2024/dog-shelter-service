using AdoptionsService.Application.DTOs;
using AdoptionsService.Domain.Enums;
using AdoptionsService.Domain.Exceptions;
using AdoptionsService.Infrastructure.Abstractions;
using AutoMapper;

namespace AdoptionsService.Application.Services;

public class ReferenceService(IUnitOfWork uow, IMapper mapper) : IReferenceService
{
    public async Task<List<RequirementDto>> GetRequirementsAsync(CancellationToken ct)
    {
        var items = await uow.Requirements.GetAllAsync(ct);
        return mapper.Map<List<RequirementDto>>(items);
    }

    public async Task<List<DogDto>> GetDogsAsync(string? status, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(status) && !DogStatuses.All.Contains(status))
            throw new ValidationException($"Status must be one of: {string.Join(", ", DogStatuses.All)}.");

        var items = await uow.Dogs.GetByStatusAsync(status, ct);
        return mapper.Map<List<DogDto>>(items);
    }

    /// <summary>Імітація події DogUpdated із DogsService (Лаб. 08 — обробник повідомлень).</summary>
    public Task SyncDogAsync(Guid id, SyncDogDto dto, CancellationToken ct) =>
        uow.Dogs.UpsertAsync(id, dto.Name.Trim(), dto.Status, ct);
}
