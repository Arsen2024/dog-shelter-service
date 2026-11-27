using AutoMapper;
using HealthService.Application.Abstractions;
using HealthService.Application.DTOs;
using HealthService.Domain.Entities;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using HealthService.Domain.ValueObjects;
using MediatR;

namespace HealthService.Application.Commands;

public record VaccineInput(string Name, string? Manufacturer, string? BatchNumber, double? DoseMl);

public record RecordVaccinationCommand(
    Guid DogId,
    DateTime VaccinatedAt,
    DateTime? NextDueAt,
    string? VeterinarianId,
    string? VeterinarianName,
    string? VeterinarianEmail,
    List<VaccineInput> Vaccines) : ICommand<VaccinationDto>;

public class RecordVaccinationCommandHandler(
    IMedicalRecordRepository records,
    IVaccinationRepository vaccinations,
    IMapper mapper) : IRequestHandler<RecordVaccinationCommand, VaccinationDto>
{
    public async Task<VaccinationDto> Handle(RecordVaccinationCommand request, CancellationToken cancellationToken)
    {
        // цілісність reference перевіряє прикладний код (FK в MongoDB немає)
        var record = await records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);

        var veterinarian = string.IsNullOrWhiteSpace(request.VeterinarianName)
            ? null
            : new Veterinarian(
                request.VeterinarianId,
                request.VeterinarianName,
                string.IsNullOrWhiteSpace(request.VeterinarianEmail) ? null : new Email(request.VeterinarianEmail));

        var vaccination = Vaccination.Create(
            request.DogId,
            record.DogName,
            record.Id,
            request.VaccinatedAt,
            request.NextDueAt,
            veterinarian,
            request.Vaccines.Select(v => new VaccineEntry(v.Name, v.Manufacturer, v.BatchNumber, v.DoseMl)));

        await vaccinations.AddAsync(vaccination, cancellationToken);

        return mapper.Map<VaccinationDto>(vaccination);
    }
}
