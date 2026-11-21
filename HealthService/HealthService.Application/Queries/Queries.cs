using HealthService.Application.DTOs;
using HealthService.Application.Mappings;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using MediatR;

namespace HealthService.Application.Queries;

public record GetMedicalRecordByDogIdQuery(Guid DogId) : IRequest<MedicalRecordDto>;

public class GetMedicalRecordByDogIdQueryHandler(IMedicalRecordRepository _records)
    : IRequestHandler<GetMedicalRecordByDogIdQuery, MedicalRecordDto>
{
    public async Task<MedicalRecordDto> Handle(GetMedicalRecordByDogIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);
        return record.ToDto();
    }
}

public record GetVaccinationsByDogIdQuery(Guid DogId) : IRequest<List<VaccinationDto>>;

public class GetVaccinationsByDogIdQueryHandler(IVaccinationRepository _vaccinations)
    : IRequestHandler<GetVaccinationsByDogIdQuery, List<VaccinationDto>>
{
    public async Task<List<VaccinationDto>> Handle(GetVaccinationsByDogIdQuery request, CancellationToken cancellationToken)
    {
        var items = await _vaccinations.GetByDogIdAsync(request.DogId, cancellationToken);
        return items.Select(v => v.ToDto()).ToList();
    }
}

public record GetOverdueVaccinationsQuery : IRequest<List<OverdueVaccinationDto>>;

public class GetOverdueVaccinationsQueryHandler(IVaccinationRepository _vaccinations)
    : IRequestHandler<GetOverdueVaccinationsQuery, List<OverdueVaccinationDto>>
{
    public async Task<List<OverdueVaccinationDto>> Handle(GetOverdueVaccinationsQuery request, CancellationToken cancellationToken)
    {
        var items = await _vaccinations.GetOverdueAsync(DateTime.UtcNow, cancellationToken);
        return items.Select(o => o.ToDto()).ToList();
    }
}

public record GetVisitByIdQuery(string Id) : IRequest<VisitDto>;

public class GetVisitByIdQueryHandler(IVisitRepository _visits) : IRequestHandler<GetVisitByIdQuery, VisitDto>
{
    public async Task<VisitDto> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken)
    {
        var visit = await _visits.GetByIdAsync(request.Id, cancellationToken)
                    ?? throw new NotFoundException("Visit", request.Id);
        return visit.ToDto();
    }
}

public record GetVisitsByDogIdQuery(Guid DogId, VisitType? Type, int PageNumber = 1, int PageSize = 20)
    : IRequest<List<VisitDto>>;

public class GetVisitsByDogIdQueryHandler(IVisitRepository _visits) : IRequestHandler<GetVisitsByDogIdQuery, List<VisitDto>>
{
    public async Task<List<VisitDto>> Handle(GetVisitsByDogIdQuery request, CancellationToken cancellationToken)
    {
        var items = await _visits.GetByDogIdAsync(request.DogId, request.Type, request.PageNumber, request.PageSize, cancellationToken);
        return items.Select(v => v.ToDto()).ToList();
    }
}
