using Microsoft.AspNetCore.Mvc;
using OrderService.API.Infrastructure.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderService.API.Infrastructure.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<OrderDTO> GetOrderByIdAsync(Guid id);
        Task<IActionResult> AddOrderAsync(OrderDTO orderDto);

        Task<(bool Success, string ErrorMessage, OrderDTO Order)> UpdateOrderAsync(OrderDTO orderDto);
        Task<Guid> DeleteOrderAsync(Guid id);
    }
}
