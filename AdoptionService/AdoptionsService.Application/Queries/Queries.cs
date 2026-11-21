using AdoptionsService.Application.DTOs;
using AdoptionsService.Application.Mappings;
using AdoptionsService.Domain.Exceptions;
using AdoptionsService.Domain.Interfaces;
using MediatR;

namespace AdoptionsService.Application.Queries;

public record GetAdoptersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<List<AdopterDto>>;

public class GetAdoptersQueryHandler(IAdopterRepository _adopters) : IRequestHandler<GetAdoptersQuery, List<AdopterDto>>
{
    public async Task<List<AdopterDto>> Handle(GetAdoptersQuery request, CancellationToken cancellationToken)
    {
        var items = await _adopters.ListAsync(request.PageNumber, request.PageSize, cancellationToken);
        return items.Select(a => a.ToDto()).ToList();
    }
}

public record GetAdopterByIdQuery(Guid Id) : IRequest<AdopterDto>;

public class GetAdopterByIdQueryHandler(IAdopterRepository _adopters) : IRequestHandler<GetAdopterByIdQuery, AdopterDto>
{
    public async Task<AdopterDto> Handle(GetAdopterByIdQuery request, CancellationToken cancellationToken)
    {
        var adopter = await _adopters.GetByIdAsync(request.Id, cancellationToken)
                      ?? throw new NotFoundException("Adopter", request.Id);
        return adopter.ToDto();
    }
}

public record GetApplicationsQuery(string? Status, Guid? AdopterId, int PageNumber = 1, int PageSize = 20)
    : IRequest<List<ApplicationListItemDto>>;

public class GetApplicationsQueryHandler(IApplicationRepository _applications)
    : IRequestHandler<GetApplicationsQuery, List<ApplicationListItemDto>>
{
    public async Task<List<ApplicationListItemDto>> Handle(GetApplicationsQuery request, CancellationToken cancellationToken)
    {
        var items = await _applications.ListAsync(
            request.Status, request.AdopterId, request.PageNumber, request.PageSize, cancellationToken);
        return items.Select(i => i.ToDto()).ToList();
    }
}

public record GetApplicationDetailsQuery(Guid Id) : IRequest<ApplicationDetailsDto>;

public class GetApplicationDetailsQueryHandler(IApplicationRepository _applications)
    : IRequestHandler<GetApplicationDetailsQuery, ApplicationDetailsDto>
{
    public async Task<ApplicationDetailsDto> Handle(GetApplicationDetailsQuery request, CancellationToken cancellationToken)
    {
        var details = await _applications.GetDetailsAsync(request.Id, cancellationToken)
                      ?? throw new NotFoundException("Application", request.Id);
        return details.ToDto();
    }
}

public record GetRequirementsQuery : IRequest<List<RequirementDto>>;

public class GetRequirementsQueryHandler(IRequirementRepository _requirements)
    : IRequestHandler<GetRequirementsQuery, List<RequirementDto>>
{
    public async Task<List<RequirementDto>> Handle(GetRequirementsQuery request, CancellationToken cancellationToken)
    {
        var items = await _requirements.GetAllAsync(cancellationToken);
        return items.Select(r => r.ToDto()).ToList();
    }
}

public record GetDogsQuery(string? Status) : IRequest<List<DogDto>>;

public class GetDogsQueryHandler(IDogRepository _dogs) : IRequestHandler<GetDogsQuery, List<DogDto>>
{
    public async Task<List<DogDto>> Handle(GetDogsQuery request, CancellationToken cancellationToken)
    {
        var items = await _dogs.ListAsync(request.Status, cancellationToken);
        return items.Select(d => d.ToDto()).ToList();
    }
}
