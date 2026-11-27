using AutoMapper;
using HealthService.Application.DTOs;
using HealthService.Domain.Entities;
using HealthService.Domain.ValueObjects;
using MongoDB.Bson;

namespace HealthService.Application.Mappings;

/// <summary>Сутності → DTO: ObjectId → string, Value Objects, вкладені документи й масиви.</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ObjectId → string (працює і для ObjectId?, і для Id, і для reference-полів)
        CreateMap<ObjectId, string>().ConvertUsing(id => id.ToString());

        // Value Objects
        CreateMap<Email, string>().ConvertUsing(e => e.Value);
        CreateMap<Money, MoneyDto>();
        CreateMap<DogSnapshot, DogSnapshotDto>();
        CreateMap<Anesthesia, AnesthesiaDto>();
        CreateMap<Veterinarian, VeterinarianDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.VetId));
        CreateMap<Allergy, AllergyDto>();
        CreateMap<ChronicCondition, ChronicConditionDto>();
        CreateMap<MedicalNote, MedicalNoteDto>();
        CreateMap<VaccineEntry, VaccineDto>();

        // Сутності
        CreateMap<VeterinaryVisit, VisitDto>();
        CreateMap<MedicalRecord, MedicalRecordDto>();
        CreateMap<Vaccination, VaccinationDto>();
    }
}
