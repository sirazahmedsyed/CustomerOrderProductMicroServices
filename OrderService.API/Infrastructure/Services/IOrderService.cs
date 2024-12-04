using Microsoft.AspNetCore.Mvc;
using OrderService.API.Infrastructure.DTOs;

namespace OrderService.API.Infrastructure.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IActionResult> GetOrderByIdAsync(Guid id);
        Task<IActionResult> AddOrderAsync(OrderDTO orderDto);
        Task<IActionResult> UpdateOrderAsync(OrderDTO orderDto);
        Task<IActionResult> DeleteOrderAsync(Guid id);
    }
}
