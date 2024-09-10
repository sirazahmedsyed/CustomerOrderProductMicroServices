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
        Task<OrderDTO> AddOrderAsync(OrderDTO orderDto);
        Task<OrderDTO> UpdateOrderAsync(OrderDTO orderDto);
        Task DeleteOrderAsync(Guid id);
    }
}
