using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedRepository.Authorization;

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
        [Route("GetAllCustomers")]
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
        [Route("GetCustomerById/{id:guid}")]
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
        [Route("CreateCustomer")]
        [Authorize(Policy = Permissions.AddCustomer)]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDTO customerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for customer creation");
                return BadRequest(ModelState);
            }
            
            try
            {
                var result = await _customerService.AddCustomerAsync(customerDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("UpdateCustomer")]
        public async Task<IActionResult> UpdateCustomer([FromBody] CustomerDTO customerDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for customer update");
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _customerService.UpdateCustomerAsync(customerDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID {CustomerId}", customerDto.CustomerId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("DeleteCustomer/{id:Guid}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer");
                return StatusCode(500, "Internal server error");
            }
        }
        }
    }
