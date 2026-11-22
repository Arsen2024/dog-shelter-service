using AdoptionsService.Application.DTOs;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Exceptions;
using AdoptionsService.Domain.Interfaces;
using AdoptionsService.Infrastructure.Abstractions;
using AutoMapper;

namespace AdoptionsService.Application.Services;

public class AdopterService(IUnitOfWork uow, IMapper mapper, ICurrentUser currentUser) : IAdopterService
{
    public async Task<List<AdopterDto>> GetPageAsync(int pageNumber, int pageSize, CancellationToken ct)
    {
        var items = await uow.Adopters.GetPageAsync(pageNumber, pageSize, ct);
        return mapper.Map<List<AdopterDto>>(items);
    }

    public async Task<AdopterDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var entity = await uow.Adopters.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("Adopter", id);
        return mapper.Map<AdopterDto>(entity);
    }

    public Task<AdopterDto> CreateAsync(CreateAdopterDto dto, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async () =>
        {
            var email = dto.Email.Trim();

            // правило, що потребує даних із БД, — у сервісі
            if (await uow.Adopters.GetByEmailAsync(email, ct) is not null)
                throw new BusinessConflictException($"Adopter with email '{email}' already exists.");

            var entity = mapper.Map<AdopterEntity>(dto);
            entity.Id = Guid.NewGuid();
            entity.FullName = dto.FullName.Trim();
            entity.Email = email;
            entity.CreatedAt = DateTime.UtcNow;
            entity.CreatedBy = currentUser.UserName;

            await uow.Adopters.AddAsync(entity, ct);

            var saved = await uow.Adopters.GetByIdAsync(entity.Id, ct)
                        ?? throw new InvalidOperationException("Adopter was not saved.");
            return mapper.Map<AdopterDto>(saved);
        }, ct: ct);

    public Task<AdopterDto> UpdateAsync(Guid id, UpdateAdopterDto dto, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async () =>
        {
            var entity = await uow.Adopters.GetByIdAsync(id, ct)
                         ?? throw new NotFoundException("Adopter", id);

            var email = dto.Email.Trim();
            if (!string.Equals(entity.Email, email, StringComparison.OrdinalIgnoreCase))
            {
                var other = await uow.Adopters.GetByEmailAsync(email, ct);
                if (other is not null && other.Id != id)
                    throw new BusinessConflictException($"Adopter with email '{email}' already exists.");
            }

            try
            {
                entity.RowVersion = Convert.FromBase64String(dto.RowVersion);
            }
            catch (FormatException)
            {
                throw new ValidationException("RowVersion must be a valid Base64 string.");
            }

            entity.FullName = dto.FullName.Trim();
            entity.Email = email;
            entity.Phone = dto.Phone;
            entity.Address = dto.Address;
            entity.UpdatedBy = currentUser.UserName;

            // оптимістична конкурентність: UPDATE ... WHERE RowVersion = @RowVersion
            if (!await uow.Adopters.UpdateAsync(entity, ct))
                throw new BusinessConflictException("The adopter was modified by another user. Reload it and try again.");

            var saved = await uow.Adopters.GetByIdAsync(id, ct)
                        ?? throw new NotFoundException("Adopter", id);
            return mapper.Map<AdopterDto>(saved);
        }, ct: ct);

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        if (!await uow.Adopters.DeleteAsync(id, ct))
            throw new NotFoundException("Adopter", id);
    }
}
