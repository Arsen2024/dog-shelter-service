using HealthService.Application.Mappings;
using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using MediatR;

namespace HealthService.Application.Commands;

public record CreateMedicalRecordCommand(Guid DogId, string DogName, string? BloodType) : IRequest<string>;

public class CreateMedicalRecordCommandHandler(IMedicalRecordRepository _records)
    : IRequestHandler<CreateMedicalRecordCommand, string>
{
    public async Task<string> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var existing = await _records.GetByDogIdAsync(request.DogId, cancellationToken);
        if (existing is not null)
            throw new BusinessRuleException($"A medical record for dog '{request.DogId}' already exists.");

        var record = new MedicalRecord
        {
            DogId = request.DogId,
            DogName = request.DogName.Trim(),
            BloodType = request.BloodType,
            CreatedAt = DateTime.UtcNow
        };

        await _records.CreateAsync(record, cancellationToken);
        return record.Id;
    }
}

public record AddAllergyCommand(Guid DogId, string Substance, AllergySeverity Severity, DateTime? NotedAt) : IRequest<Unit>;

public class AddAllergyCommandHandler(IMedicalRecordRepository _records) : IRequestHandler<AddAllergyCommand, Unit>
{
    public async Task<Unit> Handle(AddAllergyCommand request, CancellationToken cancellationToken)
    {
        var allergy = new Allergy
        {
            Substance = request.Substance.Trim(),
            Severity = request.Severity,
            NotedAt = request.NotedAt?.ToUtc() ?? DateTime.UtcNow
        };

        if (!await _records.AddAllergyAsync(request.DogId, allergy, cancellationToken))
            throw new NotFoundException("Medical record for dog", request.DogId);

        return Unit.Value;
    }
}

public record AddMedicalNoteCommand(Guid DogId, string Text) : IRequest<Unit>;

public class AddMedicalNoteCommandHandler(IMedicalRecordRepository _records, ICurrentUser _currentUser)
    : IRequestHandler<AddMedicalNoteCommand, Unit>
{
    public async Task<Unit> Handle(AddMedicalNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new MedicalNote { Text = request.Text.Trim(), Author = _currentUser.UserName, CreatedAt = DateTime.UtcNow };

        if (!await _records.AddNoteAsync(request.DogId, note, cancellationToken))
            throw new NotFoundException("Medical record for dog", request.DogId);

        return Unit.Value;
    }
}

public record RenameDogCommand(Guid DogId, string Name) : IRequest<Unit>;

public class RenameDogCommandHandler(
    IMedicalRecordRepository _records,
    IVaccinationRepository _vaccinations,
    IVisitRepository _visits) : IRequestHandler<RenameDogCommand, Unit>
{
    public async Task<Unit> Handle(RenameDogCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name.Trim();

        await _records.UpdateDogNameAsync(request.DogId, name, cancellationToken);
        await _vaccinations.UpdateDogNameAsync(request.DogId, name, cancellationToken);
        await _visits.UpdateDogNameAsync(request.DogId, name, cancellationToken);

        return Unit.Value;
    }
}
