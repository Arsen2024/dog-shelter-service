using AutoMapper;
using DogsService.Application.DTOs;
using DogsService.Domain.Exceptions;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.Specifications;

namespace DogsService.Application.Services;

public class KennelService(IUnitOfWork uow, IMapper mapper) : IKennelService
{
    public async Task<List<KennelDto>> GetAllAsync(CancellationToken ct)
    {
        var kennels = await uow.Kennels.GetAllAsync(ct);
        var result = new List<KennelDto>();

        foreach (var kennel in kennels.OrderBy(k => k.Name))
        {
            var dto = mapper.Map<KennelDto>(kennel);
            dto.Occupied = await uow.Dogs.CountAsync(new DogsByKennelSpec(kennel.Id), ct);
            result.Add(dto);
        }

        return result;
    }
}

public class FeatureService(IUnitOfWork uow, IMapper mapper) : IFeatureService
{
    public async Task<List<FeatureDto>> GetAllAsync(CancellationToken ct)
    {
        var features = await uow.Features.GetAllAsync(ct);
        return mapper.Map<List<FeatureDto>>(features.OrderBy(f => f.Name));
    }

    public async Task<List<FeatureAssignmentDto>> GetDogsAddedAfterAsync(
        Guid featureId, DateTime? addedAfter, CancellationToken ct)
    {
        _ = await uow.Features.GetByIdAsync(featureId, ct)
            ?? throw new EntityNotFoundException("Feature", featureId);

        // LINQ to Entities через проміжну сутність DogFeature (поле AssignedAt)
        var assignments = await uow.DogFeatures.GetAssignmentsAddedAfterAsync(featureId, addedAfter, ct);
        return mapper.Map<List<FeatureAssignmentDto>>(assignments);
    }
}
