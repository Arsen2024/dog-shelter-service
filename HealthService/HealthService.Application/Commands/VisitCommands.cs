using AutoMapper;
using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using HealthService.Domain.ValueObjects;
using MediatR;
using MongoDB.Bson;

namespace HealthService.Application.Commands;

public record AnesthesiaInput(string Type, int? DurationMin);

// ---------------------------------- Create ----------------------------------

public record CreateVisitCommand(
    Guid DogId,
    DateTime VisitDate,
    VisitType VisitType,
    string? VeterinarianId,
    string? VeterinarianName,
    string? VeterinarianEmail,
    List<string>? Symptoms,
    string? Diagnosis,
    List<string>? Recommendations,
    string? DogBreed,
    double? WeightKg,
    double? TemperatureC,
    AnesthesiaInput? Anesthesia,
    string? OperationNotes,
    int? TriageLevel,
    string? RelatedVisitId,
    decimal? CostAmount,
    string? CostCurrency) : ICommand<VisitDto>;

public class CreateVisitCommandHandler(
    IMedicalRecordRepository records,
    IVisitRepository visits,
    IMapper mapper) : IRequestHandler<CreateVisitCommand, VisitDto>
{
    public async Task<VisitDto> Handle(CreateVisitCommand request, CancellationToken cancellationToken)
    {
        var record = await records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);

        ObjectId? relatedId = null;
        if (request.VisitType == VisitType.FollowUp && request.RelatedVisitId is not null)
        {
            relatedId = ObjectId.Parse(request.RelatedVisitId);

            var related = await visits.GetByIdAsync(relatedId.Value, cancellationToken);
            if (related is null || related.DogId != request.DogId)
                throw new NotFoundException("Related visit for this dog", request.RelatedVisitId);
        }

        // Value Objects: інваріанти перевіряють їхні конструктори (DomainException)
        var veterinarian = string.IsNullOrWhiteSpace(request.VeterinarianName)
            ? null
            : new Veterinarian(
                request.VeterinarianId,
                request.VeterinarianName,
                string.IsNullOrWhiteSpace(request.VeterinarianEmail) ? null : new Email(request.VeterinarianEmail));

        var cost = request.CostAmount.HasValue
            ? new Money(request.CostAmount.Value, request.CostCurrency ?? "UAH")
            : null;

        var visit = VeterinaryVisit.Create(
            dogId: request.DogId,
            dogName: record.DogName,
            medicalRecordId: record.Id,
            visitDate: request.VisitDate,
            visitType: request.VisitType,
            veterinarian: veterinarian,
            symptoms: request.Symptoms,
            diagnosis: request.Diagnosis,
            recommendations: request.Recommendations,
            // гібрид: reference (MedicalRecordId) + вбудований знімок на момент візиту
            dogSnapshot: new DogSnapshot(record.DogName, request.DogBreed, request.WeightKg),
            weightKg: request.WeightKg,
            temperatureC: request.TemperatureC,
            anesthesia: request.Anesthesia is null
                ? null
                : new Anesthesia(request.Anesthesia.Type, request.Anesthesia.DurationMin),
            operationNotes: request.OperationNotes,
            triageLevel: request.TriageLevel,
            relatedVisitId: relatedId,
            cost: cost);

        await visits.AddAsync(visit, cancellationToken);

        return mapper.Map<VisitDto>(visit);
    }
}

// ---------------------------------- Update ----------------------------------

public record UpdateVisitCommand(
    string Id,
    long Version,
    string? Diagnosis,
    List<string>? Symptoms,
    List<string>? Recommendations,
    string? OperationNotes,
    decimal? CostAmount,
    string? CostCurrency) : ICommand<VisitDto>;

public class UpdateVisitCommandHandler(IVisitRepository visits, IMapper mapper)
    : IRequestHandler<UpdateVisitCommand, VisitDto>
{
    public async Task<VisitDto> Handle(UpdateVisitCommand request, CancellationToken cancellationToken)
    {
        var id = ObjectId.Parse(request.Id);

        var visit = await visits.GetByIdAsync(id, cancellationToken)
                    ?? throw new NotFoundException("Visit", request.Id);

        // швидка перевірка версії; остаточну гарантію дає ReplaceOne з умовою на version
        if (visit.Version != request.Version)
            throw new ConflictException(
                $"Version conflict: the visit has version {visit.Version}, but {request.Version} was expected.");

        var cost = request.CostAmount.HasValue
            ? new Money(request.CostAmount.Value, request.CostCurrency ?? "UAH")
            : null;

        visit.Update(request.Diagnosis, request.Symptoms, request.Recommendations, request.OperationNotes, cost);

        await visits.UpdateAsync(visit, cancellationToken);

        return mapper.Map<VisitDto>(visit);
    }
}

// ---------------------------------- Delete ----------------------------------

public record DeleteVisitCommand(string Id) : ICommand;

public class DeleteVisitCommandHandler(IVisitRepository visits) : IRequestHandler<DeleteVisitCommand, Unit>
{
    public async Task<Unit> Handle(DeleteVisitCommand request, CancellationToken cancellationToken)
    {
        if (!await visits.DeleteAsync(ObjectId.Parse(request.Id), cancellationToken))
            throw new NotFoundException("Visit", request.Id);

        return Unit.Value;
    }
}
