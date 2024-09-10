using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Services;

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

        // GET: api/customer
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

        // GET: api/customer/{id}
        [HttpGet]
        [Route("{id:int}")]
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

        // POST: api/customer
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
                var createdCustomer = await _customerService.AddCustomerAsync(customerDto);
                _logger.LogInformation("Customer with ID {CustomerId} created", customerDto.CustomerId);
                return Ok(createdCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer with ID {CustomerId}", customerDto.CustomerId);
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
                var updatedCustomer = await _customerService.UpdateCustomerAsync(customerDto);
                if (!updatedCustomer.IsSuccess)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", customerDto.CustomerId);
                    return NotFound($"Customer with ID {customerDto.CustomerId} not found.");
                }

                _logger.LogInformation("Customer with ID {CustomerId} updated", customerDto.CustomerId);
                return Ok(updatedCustomer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID {CustomerId}", customerDto.CustomerId);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/customer/{id}
        [HttpDelete]
        [Route("{id:int}")]
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




