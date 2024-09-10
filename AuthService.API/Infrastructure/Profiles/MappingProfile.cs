using AuthMicroservice.Infrastructure.DTOs;
using AuthMicroservice.Infrastructure.Entities;
using AutoMapper;
namespace AuthMicroservice.Infrastructure.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.UserCode));
        }
    }
}
