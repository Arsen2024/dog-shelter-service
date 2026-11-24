using AutoMapper;
using DogsService.Application.DTOs;
using DogsService.Domain.Entities;
using DogsService.Domain.Enums;
using DogsService.Domain.Exceptions;
using DogsService.Infrastructure.Abstractions;
using DogsService.Infrastructure.Specifications;
using FluentValidation;

namespace DogsService.Application.Services;

/// <summary>
/// Бізнес-операції працюють лише через Unit of Work. Правила сутності перевіряє сама сутність;
/// сервіс — правила з даними з БД (унікальність імені, наявність породи/вольєра, місткість вольєра).
/// </summary>
public class DogService(
    IUnitOfWork uow,
    IMapper mapper,
    IValidator<CreateDogDto> createValidator,
    IValidator<ChangeWeightDto> weightValidator) : IDogService
{
    // ------------------------- читання -------------------------

    public async Task<PagedResult<DogDto>> GetPageAsync(DogListQuery query, CancellationToken ct)
    {
        DogStatus? status = Enum.TryParse<DogStatus>(query.Status, ignoreCase: true, out var parsed) ? parsed : null;
        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);

        var spec = new DogsFilterSpec(
            query.BreedId, query.KennelId, status, query.Search?.Trim(),
            query.SortBy, descending, query.Page, query.PageSize);

        var items = await uow.Dogs.ListAsync(spec, ct);        // лише сторінка
        var totalCount = await uow.Dogs.CountAsync(spec, ct);  // усі записи за фільтром

        return PagedResult<DogDto>.Create(mapper.Map<List<DogDto>>(items), totalCount, query.Page, query.PageSize);
    }

    public async Task<DogDetailsDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var dog = await uow.Dogs.GetDetailsAsync(id, ct)       // Eager Loading: один запит
                  ?? throw new EntityNotFoundException("Dog", id);

        return mapper.Map<DogDetailsDto>(dog);
    }

    // ------------------------- створення -------------------------

    public async Task<DogDetailsDto> CreateAsync(CreateDogDto dto, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(dto, ct);

        var name = dto.Name.Trim();

        if (await uow.Dogs.AnyAsync(new DogByNameSpec(name), ct))
            throw new ConflictException($"A dog named '{name}' already exists.");

        _ = await uow.Breeds.GetByIdAsync(dto.BreedId, ct)
            ?? throw new EntityNotFoundException("Breed", dto.BreedId);

        if (dto.KennelId is { } kennelId)
            await EnsureKennelHasRoomAsync(kennelId, ct);

        var color = string.Join(" & ", dto.Colors.Select(c => c.Trim().ToLowerInvariant()));

        // конструктор сутності перевіряє інваріанти (DomainException → 409)
        var dog = new DogEntity(name, color, dto.TailLength, dto.Weight, dto.BreedId, dto.KennelId);

        if (dto.Profile is { } profile)
            dog.SetProfile(profile.BirthDate, profile.Gender, profile.Description, profile.Temperament);

        await uow.Dogs.AddAsync(dog, ct);
        await uow.SaveChangesAsync(ct);                        // єдиний SaveChanges наприкінці операції

        return await GetByIdAsync(dog.Id, ct);
    }

    // ------------------------- зміни -------------------------

    public async Task ChangeWeightAsync(Guid id, ChangeWeightDto dto, CancellationToken ct)
    {
        await weightValidator.ValidateAndThrowAsync(dto, ct);

        var dog = await GetTrackedAsync(id, ct);
        dog.ChangeWeight(dto.Weight);
        await uow.SaveChangesAsync(ct);
    }

    public async Task ChangeStatusAsync(Guid id, ChangeStatusDto dto, CancellationToken ct)
    {
        var dog = await GetTrackedAsync(id, ct);
        dog.ChangeStatus(Enum.Parse<DogStatus>(dto.Status, ignoreCase: true));   // перехід перевіряє сутність
        await uow.SaveChangesAsync(ct);
    }

    public async Task AssignKennelAsync(Guid id, AssignKennelDto dto, CancellationToken ct)
    {
        var dog = await GetTrackedAsync(id, ct);

        if (dog.KennelId != dto.KennelId)
            await EnsureKennelHasRoomAsync(dto.KennelId, ct);

        dog.AssignToKennel(dto.KennelId);
        await uow.SaveChangesAsync(ct);
    }

    public async Task RemoveFromKennelAsync(Guid id, CancellationToken ct)
    {
        var dog = await GetTrackedAsync(id, ct);
        dog.RemoveFromKennel();
        await uow.SaveChangesAsync(ct);
    }

    public async Task AddFeatureAsync(Guid id, AddFeatureDto dto, CancellationToken ct)
    {
        _ = await GetTrackedAsync(id, ct);

        _ = await uow.Features.GetByIdAsync(dto.FeatureId, ct)
            ?? throw new EntityNotFoundException("Feature", dto.FeatureId);

        if (await uow.DogFeatures.GetAsync(id, dto.FeatureId, ct) is not null)
            throw new ConflictException("The dog already has this feature.");

        await uow.DogFeatures.AddAsync(new DogFeatureEntity(id, dto.FeatureId, dto.Notes), ct);
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var dog = await GetTrackedAsync(id, ct);
        uow.Dogs.Delete(dog);                                  // AppDbContext перетворює на soft delete
        await uow.SaveChangesAsync(ct);
    }

    // ------------------------- допоміжні -------------------------

    private async Task<DogEntity> GetTrackedAsync(Guid id, CancellationToken ct) =>
        await uow.Dogs.GetByIdAsync(id, ct) ?? throw new EntityNotFoundException("Dog", id);

    private async Task EnsureKennelHasRoomAsync(Guid kennelId, CancellationToken ct)
    {
        var kennel = await uow.Kennels.GetByIdAsync(kennelId, ct)
                     ?? throw new EntityNotFoundException("Kennel", kennelId);

        var occupied = await uow.Dogs.CountAsync(new DogsByKennelSpec(kennelId), ct);

        if (occupied >= kennel.Capacity)
            throw new ConflictException($"Kennel '{kennel.Name}' is full ({occupied}/{kennel.Capacity}).");
    }
}
