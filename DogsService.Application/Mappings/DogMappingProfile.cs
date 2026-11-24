using AutoMapper;
using DogsService.Application.DTOs;
using DogsService.Domain.Entities;
using DogsService.Domain.Models;

namespace DogsService.Application.Mappings;

public class DogMappingProfile : Profile
{
    public DogMappingProfile()
    {
        CreateMap<DogEntity, DogDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.BreedName, o => o.MapFrom(s => s.Breed != null ? s.Breed.Name : null))
            .ForMember(d => d.KennelName, o => o.MapFrom(s => s.Kennel != null ? s.Kennel.Name : null));

        CreateMap<DogProfileEntity, DogProfileDto>();

        CreateMap<DogFeatureEntity, DogFeatureDto>()
            .ForMember(d => d.FeatureName, o => o.MapFrom(s => s.Feature != null ? s.Feature.Name : null))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.Feature != null ? s.Feature.Description : null));

        CreateMap<DogEntity, DogDetailsDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.BreedName, o => o.MapFrom(s => s.Breed != null ? s.Breed.Name : null))
            .ForMember(d => d.KennelName, o => o.MapFrom(s => s.Kennel != null ? s.Kennel.Name : null))
            .ForMember(d => d.Features, o => o.MapFrom(s => s.DogFeatures));

        CreateMap<BreedEntity, BreedDto>();

        CreateMap<DogEntity, BreedDogDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

        CreateMap<BreedEntity, BreedWithDogsDto>();

        CreateMap<KennelEntity, KennelDto>()
            .ForMember(d => d.Occupied, o => o.Ignore());

        CreateMap<FeatureEntity, FeatureDto>();

        CreateMap<FeatureAssignmentModel, FeatureAssignmentDto>();
    }
}
