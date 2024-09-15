using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace OrderService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                _logger.LogInformation("Getting all orders");
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all orders");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting order with ID {OrderId}", id);
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    _logger.LogWarning("Order with ID {OrderId} not found", id);
                    return NotFound($"Order with OrderId {id} not found");
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting order with ID {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

       [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderDTO orderDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order creation");
                return BadRequest(ModelState);
            }

            try
            {
                var createdAtOrder = await _orderService.AddOrderAsync(orderDto);
                _logger.LogInformation("Order with ID {OrderId} created", orderDto.OrderId);
                return Ok(createdAtOrder);
            }
            catch (CustomerNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ProductNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating order with ID {OrderId}", orderDto.OrderId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("")]
        [Consumes("application/json")]
        public async Task<IActionResult> UpdateOrder([FromBody] OrderDTO orderDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order update");
                return BadRequest(ModelState);
            }

            try
            {
                await _orderService.UpdateOrderAsync(orderDto);
                _logger.LogInformation("Order with ID {OrderId} updated", orderDto.OrderId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating order with ID {OrderId}", orderDto.OrderId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting order with ID {OrderId}", id);
                await _orderService.DeleteOrderAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting order with ID {OrderId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
