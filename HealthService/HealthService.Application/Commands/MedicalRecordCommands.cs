using AutoMapper;
using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using HealthService.Domain.ValueObjects;
using MediatR;

namespace HealthService.Application.Commands;

public record CreateMedicalRecordCommand(Guid DogId, string DogName, string? BloodType) : ICommand<MedicalRecordDto>;

public class CreateMedicalRecordCommandHandler(IMedicalRecordRepository records, IMapper mapper)
    : IRequestHandler<CreateMedicalRecordCommand, MedicalRecordDto>
{
    public async Task<MedicalRecordDto> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var record = MedicalRecord.Create(request.DogId, request.DogName, request.BloodType);

        await records.AddAsync(record, cancellationToken);

        return mapper.Map<MedicalRecordDto>(record);
    }
}

public record AddAllergyCommand(Guid DogId, string Substance, AllergySeverity Severity, DateTime? NotedAt) : ICommand;

public class AddAllergyCommandHandler(IMedicalRecordRepository records) : IRequestHandler<AddAllergyCommand, Unit>
{
    public async Task<Unit> Handle(AddAllergyCommand request, CancellationToken cancellationToken)
    {
        var record = await records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);

        record.AddAllergy(new Allergy(request.Substance, request.Severity, request.NotedAt ?? DateTime.UtcNow));

        await records.UpdateAsync(record, cancellationToken);
        return Unit.Value;
    }
}

public record AddMedicalNoteCommand(Guid DogId, string Text) : ICommand;

public class AddMedicalNoteCommandHandler(IMedicalRecordRepository records, ICurrentUser currentUser)
    : IRequestHandler<AddMedicalNoteCommand, Unit>
{
    public async Task<Unit> Handle(AddMedicalNoteCommand request, CancellationToken cancellationToken)
    {
        var record = await records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);

        record.AddNote(new MedicalNote(request.Text, currentUser.UserName, DateTime.UtcNow));

        await records.UpdateAsync(record, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Синхронізація денормалізованого dogName (у Лаб. 08 — обробник події DogUpdated).</summary>
public record RenameDogCommand(Guid DogId, string Name) : ICommand;

public class RenameDogCommandHandler(
    IMedicalRecordRepository records,
    IVaccinationRepository vaccinations,
    IVisitRepository visits) : IRequestHandler<RenameDogCommand, Unit>
{
    public async Task<Unit> Handle(RenameDogCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        // dogName — копія для читання (оновлюється); dogSnapshot у візитах — знімок (НЕ оновлюється)
        await records.RenameDogAsync(request.DogId, name, cancellationToken);
        await vaccinations.RenameDogAsync(request.DogId, name, cancellationToken);
        await visits.RenameDogAsync(request.DogId, name, cancellationToken);

        return Unit.Value;
    }
}
