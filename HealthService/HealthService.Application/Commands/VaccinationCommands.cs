using HealthService.Application.Mappings;
using HealthService.Domain.Entities;
using HealthService.Domain.Exceptions;
using HealthService.Domain.Interfaces;
using MediatR;

namespace HealthService.Application.Commands;

public record VaccineInput(string Name, string? Manufacturer, string? BatchNumber, double? DoseMl);

public record RecordVaccinationCommand(
    Guid DogId,
    DateTime VaccinatedAt,
    DateTime? NextDueAt,
    string? VeterinarianId,
    string? VeterinarianName,
    List<VaccineInput> Vaccines) : IRequest<string>;

public class RecordVaccinationCommandHandler(IMedicalRecordRepository _records, IVaccinationRepository _vaccinations)
    : IRequestHandler<RecordVaccinationCommand, string>
{
    public async Task<string> Handle(RecordVaccinationCommand request, CancellationToken cancellationToken)
    {
        // Цілісність reference перевіряє прикладний код (FK в MongoDB немає)
        var record = await _records.GetByDogIdAsync(request.DogId, cancellationToken)
                     ?? throw new NotFoundException("Medical record for dog", request.DogId);

        var vaccination = new Vaccination
        {
            DogId = request.DogId,
            DogName = record.DogName,
            MedicalRecordId = record.Id,
            VaccinatedAt = request.VaccinatedAt.ToUtc(),
            NextDueAt = request.NextDueAt?.ToUtc(),
            Veterinarian = string.IsNullOrWhiteSpace(request.VeterinarianName)
                ? null
                : new Veterinarian { VetId = request.VeterinarianId, Name = request.VeterinarianName.Trim() },
            Vaccines = request.Vaccines
                .Select(v => new VaccineEntry
                {
                    Name = v.Name.Trim(),
                    Manufacturer = v.Manufacturer,
                    BatchNumber = v.BatchNumber,
                    DoseMl = v.DoseMl
                }).ToList()
        };

        await _vaccinations.CreateAsync(vaccination, cancellationToken);
        return vaccination.Id;
    }
}
