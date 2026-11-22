using AdoptionsService.Application.DTOs;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Enums;
using AdoptionsService.Domain.Exceptions;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure.Abstractions;
using AutoMapper;

namespace AdoptionsService.Application.Services;

public class ApplicationService(IUnitOfWork uow, IMapper mapper, ICurrentUser currentUser) : IApplicationService
{
    // ------------------------- читання -------------------------

    public async Task<List<ApplicationListItemDto>> GetPageAsync(ApplicationListQuery query, CancellationToken ct)
    {
        var items = await uow.Applications.GetPageAsync(
            query.Status, query.AdopterId, query.PageNumber, query.PageSize, ct);
        return mapper.Map<List<ApplicationListItemDto>>(items);
    }

    public async Task<ApplicationDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await uow.Applications.GetWithDogsAsync(id, ct)
                     ?? throw new NotFoundException("Application", id);
        return mapper.Map<ApplicationDto>(entity);
    }

    public async Task<ApplicationDetailsDto> GetDetailsAsync(Guid id, CancellationToken ct)
    {
        var details = await uow.Applications.GetDetailsAsync(id, ct)
                      ?? throw new NotFoundException("Application", id);
        return mapper.Map<ApplicationDetailsDto>(details);
    }

    public async Task<List<StatusHistoryDto>> GetHistoryAsync(Guid id, CancellationToken ct)
    {
        if (await uow.Applications.GetByIdAsync(id, ct) is null)
            throw new NotFoundException("Application", id);

        var history = await uow.History.GetByApplicationAsync(id, ct);   // чистий ADO.NET
        return mapper.Map<List<StatusHistoryDto>>(history);
    }

    // ------------------------- операції (процедури) -------------------------

    public async Task<ApplicationDto> CreateAsync(CreateApplicationDto dto, CancellationToken ct)
    {
        // усі перевірки (заявник, собака, доступність) і транзакція — в usp_CreateApplication
        var id = await uow.Applications.CreateWithDogAsync(dto.AdopterId, dto.DogId, currentUser.UserName, ct);
        return await GetByIdAsync(id, ct);
    }

    public Task AddDogAsync(Guid id, AddDogDto dto, CancellationToken ct) =>
        uow.Applications.AddDogAsync(id, dto.DogId, dto.Priority, ct);

    public Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, CancellationToken ct) =>
        uow.Applications.ChangeStatusAsync(id, dto.NewStatus, currentUser.UserName, dto.Comment, ct);

    public async Task FulfillRequirementAsync(Guid id, Guid requirementId, FulfillRequirementDto dto, CancellationToken ct)
    {
        var updated = await uow.Applications.FulfillRequirementAsync(id, requirementId, currentUser.UserName, dto.Notes, ct);

        if (!updated)
            throw new NotFoundException(
                $"Requirement '{requirementId}' is not attached to an open application '{id}'.");
    }

    // ------------------------- транзакційна бізнес-операція -------------------------

    public Task SignAgreementAsync(Guid id, SignAgreementDto dto, bool simulateFailure, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async () =>
        {
            // блокуємо рядок заявки до кінця транзакції (UPDLOCK, HOLDLOCK)
            var application = await uow.Applications.GetForUpdateAsync(id, ct)
                              ?? throw new NotFoundException("Application", id);

            if (application.Status != ApplicationStatuses.Approved)
                throw new BusinessConflictException(
                    $"An agreement can be signed only for an Approved application (current status: {application.Status}).");

            if (!await uow.Applications.HasDogAsync(id, dto.DogId, ct))
                throw new BusinessConflictException("The dog is not part of this application.");

            var number = dto.AgreementNumber.Trim();
            var signedBy = dto.SignedBy.Trim();
            var now = DateTime.UtcNow;

            // 1) договір                  — репозиторій заявок (Dapper)
            await uow.Applications.AddAgreementAsync(new AdoptionAgreementEntity
            {
                ApplicationId = id,
                DogId = dto.DogId,
                AgreementNumber = number,
                SignedAt = now,
                SignedBy = signedBy,
                Terms = dto.Terms
            }, ct);

            // 2) статус заявки            — репозиторій заявок (Dapper)
            await uow.Applications.SetStatusAsync(id, ApplicationStatuses.Completed, currentUser.UserName, ct);

            // 3) запис в історію статусів — окремий репозиторій (чистий ADO.NET)
            await uow.History.AddAsync(new ApplicationStatusHistoryEntity
            {
                ApplicationId = id,
                OldStatus = application.Status,
                NewStatus = ApplicationStatuses.Completed,
                ChangedAt = now,
                ChangedBy = currentUser.UserName,
                Comment = $"Agreement {number} signed"
            }, ct);

            // штучна помилка ПІСЛЯ записів у різних репозиторіях — для демонстрації відкату
            if (simulateFailure)
                throw new BusinessConflictException(
                    "Simulated failure after all writes: the transaction was rolled back, nothing was saved.");

            return true;
        }, ct: ct);
}
