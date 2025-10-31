using AutoMapper;
using DogsService.Application.DTOs.Dog;
using DogsService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DogsService.Application.Mappings
{
    public class DogMappingProfile : Profile
    {
        public DogMappingProfile() 
        { 
            CreateMap<DogEntity, DogDto>();
        }
    }
}
