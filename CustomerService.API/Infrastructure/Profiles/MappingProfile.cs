using AutoMapper;
using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CustomerService.API.Infrastructure.Profiles
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
