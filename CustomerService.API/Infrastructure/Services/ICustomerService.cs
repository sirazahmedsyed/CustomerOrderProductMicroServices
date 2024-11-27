using CustomerService.API.Infrastructure.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CustomerService.API.Infrastructure.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync();
        Task<CustomerDTO> GetCustomerByIdAsync(Guid customerId);
        Task<IActionResult> AddCustomerAsync(CustomerDTO customerDto);
        Task<(bool IsSuccess, CustomerDTO Customer, string Message)> UpdateCustomerAsync(CustomerDTO customerDto);
        Task<(bool IsSuccess, string Message)> DeleteCustomerAsync(Guid customerId);
    }
}
