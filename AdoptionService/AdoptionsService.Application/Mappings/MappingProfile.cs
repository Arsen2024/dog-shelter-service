using AdoptionsService.Application.DTOs;
using AdoptionsService.Domain.Entities;
using AdoptionsService.Domain.Models;
using AutoMapper;

namespace AdoptionsService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ---- заявники ----
        CreateMap<AdopterEntity, AdopterDto>()
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => Convert.ToBase64String(s.RowVersion)));

        CreateMap<CreateAdopterDto, AdopterEntity>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.CreatedBy, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedBy, o => o.Ignore())
            .ForMember(d => d.IsDeleted, o => o.Ignore())
            .ForMember(d => d.RowVersion, o => o.Ignore());

        // ---- заявки ----
        CreateMap<ApplicationDogEntity, ApplicationDogDto>();

        CreateMap<AdoptionApplicationEntity, ApplicationDto>()
            .ForMember(d => d.RowVersion, o => o.MapFrom(s => Convert.ToBase64String(s.RowVersion)));

        CreateMap<ApplicationListItemModel, ApplicationListItemDto>();

        CreateMap<ApplicationDogModel, ApplicationDetailsDogDto>();
        CreateMap<ApplicationRequirementModel, ApplicationRequirementDto>();
        CreateMap<StatusHistoryModel, StatusHistoryDto>();
        CreateMap<ApplicationStatusHistoryEntity, StatusHistoryDto>();

        CreateMap<ApplicationDetailsModel, ApplicationDetailsDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Header.Id))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Header.Status))
            .ForMember(d => d.SubmittedAt, o => o.MapFrom(s => s.Header.SubmittedAt))
            .ForMember(d => d.DecisionNote, o => o.MapFrom(s => s.Header.DecisionNote))
            .ForMember(d => d.AdopterId, o => o.MapFrom(s => s.Header.AdopterId))
            .ForMember(d => d.AdopterName, o => o.MapFrom(s => s.Header.FullName))
            .ForMember(d => d.AdopterEmail, o => o.MapFrom(s => s.Header.Email))
            .ForMember(d => d.AgreementNumber, o => o.MapFrom(s => s.Header.AgreementNumber))
            .ForMember(d => d.AgreementSignedAt, o => o.MapFrom(s => s.Header.SignedAt));

        // ---- довідники ----
        CreateMap<RequirementEntity, RequirementDto>();
        CreateMap<DogEntity, DogDto>();
    }
}
