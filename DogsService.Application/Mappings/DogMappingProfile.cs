using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Domain.Entities;

namespace DogsService.Application.Mappings;

public class DogMappingProfile : Profile
{
    public DogMappingProfile()
    {
        CreateMap<DogEntity, DogDto>();
    }
}