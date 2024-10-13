using AutoMapper;
using UserGroupService.API.Infrastructure.DTOs;
using UserGroupService.API.Infrastructure.Entities;

namespace UserGroupService.API.Infrastructure.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserGroup, UserGroupDTO>();
            CreateMap<CreateUserGroupDTO, UserGroup>();
            CreateMap<UpdateUserGroupDTO, UserGroup>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
