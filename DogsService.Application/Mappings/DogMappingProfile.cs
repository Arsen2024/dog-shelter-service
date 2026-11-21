using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Domain.Entities;

namespace DogsService.Application.Mappings;

public class DogMappingProfile : Profile
{
    public DogMappingProfile()
    {
        CreateMap<DogEntity, DogDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.BreedName, o => o.MapFrom(s => s.Breed != null ? s.Breed.Name : null))
            .ForMember(d => d.KennelName, o => o.MapFrom(s => s.Kennel != null ? s.Kennel.Name : null));
    }
}
