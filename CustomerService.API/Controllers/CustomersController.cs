using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using CustomerService.API.Infrastructure.Entities;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAllCustomers()
        {
            try
            {
                _logger.LogInformation("Getting all customers");
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all customers");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("{id:Guid}")]
        public async Task<IActionResult> GetCustomerById(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting customer with ID {CustomerId}", id);
                var customerResult = await _customerService.GetCustomerByIdAsync(id);
                if (customerResult == null)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                    return NotFound($"Customer with ID {id} not found.");
                }
                return Ok(customerResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting customer with ID {CustomerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDTO customerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for customer creation");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, customerId, createdCustomerDto, message) = await _customerService.AddCustomerAsync(customerDto);

                if (!isSuccess)
                {
                    _logger.LogWarning("Customer creation failed: {Message}", message);
                    return Conflict(new { Message = message });
                }

                _logger.LogInformation("Customer with ID {CustomerId} created", customerId);
                return Ok(createdCustomerDto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateCustomer([FromBody] CustomerDTO customerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for customer update");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, updatedCustomerDto, message) = await _customerService.UpdateCustomerAsync(customerDto);

                if (!isSuccess)
                {
                    _logger.LogWarning("Customer updation failed: {Message}", message);
                    return Conflict(new { Message = message });
                }

                _logger.LogInformation("Customer with ID {CustomerId} updated", updatedCustomerDto.CustomerId);
                return Ok(updatedCustomerDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID {CustomerId}", customerDto.CustomerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("{id:Guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting customer with ID {CustomerId}", id);
                var deletedCustomer = await _customerService.DeleteCustomerAsync(id);
                if (!deletedCustomer.IsSuccess)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", id);
                    return NotFound($"Customer with ID {id} not found.");
                }

                _logger.LogInformation("Customer with ID {CustomerId} deleted", id);
                return Ok(deletedCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting customer with ID {CustomerId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
