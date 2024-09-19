using AutoMapper;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductService.API.Infrastructure.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Customer, CustomerDTO>();
            CreateMap<CustomerDTO, Customer>();
        }
    }
}
