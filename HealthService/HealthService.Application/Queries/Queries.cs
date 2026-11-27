using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using MediatR;
using MongoDB.Bson;

namespace HealthService.Application.Queries;

// Обробники запитів звертаються лише до IXxxQueries; IMongoCollection в Application недоступний.

public record GetVisitByIdQuery(string Id) : IQuery<VisitDto>;

public class GetVisitByIdQueryHandler(IVisitQueries queries) : IRequestHandler<GetVisitByIdQuery, VisitDto>
{
    public async Task<VisitDto> Handle(GetVisitByIdQuery request, CancellationToken cancellationToken) =>
        await queries.GetByIdAsync(ObjectId.Parse(request.Id), cancellationToken)
        ?? throw new NotFoundException("Visit", request.Id);
}

public record GetVisitsListQuery(
    Guid? DogId, VisitType? Type, string? Search, int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<VisitListItemDto>>;

public class GetVisitsListQueryHandler(IVisitQueries queries)
    : IRequestHandler<GetVisitsListQuery, PagedResult<VisitListItemDto>>
{
    public Task<PagedResult<VisitListItemDto>> Handle(GetVisitsListQuery request, CancellationToken cancellationToken) =>
        queries.GetListAsync(request.DogId, request.Type, request.Search?.Trim(), request.Page, request.PageSize, cancellationToken);
}

public record GetMedicalRecordByDogIdQuery(Guid DogId) : IQuery<MedicalRecordDto>;

public class GetMedicalRecordByDogIdQueryHandler(IMedicalRecordQueries queries)
    : IRequestHandler<GetMedicalRecordByDogIdQuery, MedicalRecordDto>
{
    public async Task<MedicalRecordDto> Handle(GetMedicalRecordByDogIdQuery request, CancellationToken cancellationToken) =>
        await queries.GetByDogIdAsync(request.DogId, cancellationToken)
        ?? throw new NotFoundException("Medical record for dog", request.DogId);
}

public record GetVaccinationsByDogIdQuery(Guid DogId) : IQuery<List<VaccinationDto>>;

public class GetVaccinationsByDogIdQueryHandler(IVaccinationQueries queries)
    : IRequestHandler<GetVaccinationsByDogIdQuery, List<VaccinationDto>>
{
    public Task<List<VaccinationDto>> Handle(GetVaccinationsByDogIdQuery request, CancellationToken cancellationToken) =>
        queries.GetByDogIdAsync(request.DogId, cancellationToken);
}

public record GetOverdueVaccinationsQuery : IQuery<List<OverdueVaccinationDto>>;

public class GetOverdueVaccinationsQueryHandler(IVaccinationQueries queries)
    : IRequestHandler<GetOverdueVaccinationsQuery, List<OverdueVaccinationDto>>
{
    public Task<List<OverdueVaccinationDto>> Handle(GetOverdueVaccinationsQuery request, CancellationToken cancellationToken) =>
        queries.GetOverdueAsync(DateTime.UtcNow, cancellationToken);
}
