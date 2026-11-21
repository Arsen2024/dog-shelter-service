using HealthService.Application.Mappings;
using HealthService.Domain.Entities;
using HealthService.Domain.Enums;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using MediatR;

namespace HealthService.Application.Commands;

public record AnesthesiaInput(string Type, int? DurationMin);

public record RecordVisitCommand(
    Guid DogId,
    DateTime VisitDate,
    VisitType VisitType,
    string? VeterinarianId,
    string? VeterinarianName,
    List<string>? Symptoms,
    string? Diagnosis,
    List<string>? Recommendations,
    string? DogBreed,
    double? WeightKg,
    double? TemperatureC,
    AnesthesiaInput? Anesthesia,
    string? OperationNotes,
    int? TriageLevel,
    string? RelatedVisitId) : IRequest<string>;

public class RecordVisitCommandHandler(IMedicalRecordRepository _records, IVisitRepository _visits)
    : IRequestHandler<RecordVisitCommand, string>
{
    public async Task<string> Handle(RecordVisitCommand request, CancellationToken cancellationToken)
    {
        var record = await _records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);

        if (request.VisitType == VisitType.FollowUp && request.RelatedVisitId is not null)
        {
            var related = await _visits.GetByIdAsync(request.RelatedVisitId, cancellationToken);
            if (related is null || related.DogId != request.DogId)
                throw new NotFoundException("Related visit for this dog", request.RelatedVisitId);
        }

        var visit = new VeterinaryVisit
        {
            DogId = request.DogId,
            DogName = record.DogName,
            MedicalRecordId = record.Id,
            RelatedVisitId = request.RelatedVisitId,
            // гібрид: reference (MedicalRecordId) + вбудований знімок на момент візиту
            DogSnapshot = new DogSnapshot { Name = record.DogName, Breed = request.DogBreed, WeightKg = request.WeightKg },
            VisitDate = request.VisitDate.ToUtc(),
            VisitType = request.VisitType,
            Veterinarian = string.IsNullOrWhiteSpace(request.VeterinarianName)
                ? null
                : new Veterinarian { VetId = request.VeterinarianId, Name = request.VeterinarianName.Trim() },
            Symptoms = request.Symptoms ?? [],
            Diagnosis = request.Diagnosis,
            Recommendations = request.Recommendations ?? [],
            WeightKg = request.WeightKg,
            TemperatureC = request.TemperatureC,
            Anesthesia = request.Anesthesia is null
                ? null
                : new Anesthesia { Type = request.Anesthesia.Type, DurationMin = request.Anesthesia.DurationMin },
            OperationNotes = request.OperationNotes,
            TriageLevel = request.TriageLevel
        };

        await _visits.CreateAsync(visit, cancellationToken);
        return visit.Id;
    }
}
