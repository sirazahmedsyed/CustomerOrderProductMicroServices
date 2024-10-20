using AutoMapper;
using PurchaseService.API.Infrastructure.DTOs;
using PurchaseService.API.Infrastructure.Entities;

namespace PurchaseService.API.Infrastructure.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Purchase, PurchaseDTO>();
            CreateMap<PurchaseDTO, Purchase>();
        }
    }
}
