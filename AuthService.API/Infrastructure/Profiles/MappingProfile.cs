using AuthService.API.Infrastructure.DTOs;
using AuthService.API.Infrastructure.Entities;
using AutoMapper;
namespace AuthService.API.Infrastructure.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.UserNo, opt => opt.MapFrom(src => src.UserNo));
        }
    }
}
