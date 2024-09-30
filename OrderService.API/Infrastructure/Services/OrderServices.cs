using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
namespace OrderService.API.Infrastructure.Services
{
public class OrderServices : IOrderService
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                include: q => q.Include(o => o.OrderItems));
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<OrderDTO> GetOrderByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id,
                include: q => q.Include(o => o.OrderItems));
            return order == null ? null : _mapper.Map<OrderDTO>(order);
        }

        public async Task<(bool Success, string ErrorMessage, OrderDTO Order)> AddOrderAsync(OrderDTO orderDto)
        {
            // 1. Validate customer
            if (!await _unitOfWork.Repository<Order>().CustomerExistsAsync(orderDto.CustomerId))
            {
                return (false, $"Customer with ID {orderDto.CustomerId} does not exist.", null);
            }

            // 2. Validate all products
            var productDetailsCache = new Dictionary<int, (decimal Price, decimal TaxPercentage)>();
            foreach (var item in orderDto.OrderItems)
            {
                try
                {
                    var details = await _unitOfWork.Repository<Order>().GetProductDetailsAsync(item.ProductId);
                    productDetailsCache[item.ProductId] = details;
                }
                catch (ArgumentException)
                {
                    return (false, $"Product with ID {item.ProductId} does not exist.", null);
                }
            }
                // If we've reached this point, both customer and all products are valid
                // Proceed with order processing
            var existingOrder = (await _unitOfWork.Repository<Order>()
                .GetAllAsync(
                    o => o.CustomerId == orderDto.CustomerId &&
                    o.OrderItems.Any(oi => orderDto.OrderItems.Select(i => i.ProductId).Contains(oi.ProductId)),
                    include: q => q.Include(o => o.OrderItems)))
                .FirstOrDefault();

            if (existingOrder != null)
            {
                // Update existing order
                foreach (var itemDto in orderDto.OrderItems)
                {
                    var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += itemDto.Quantity;
                    }
                    else
                    {
                        var (price, _) = productDetailsCache[itemDto.ProductId];
                        existingOrder.OrderItems.Add(new OrderItem
                        {
                            ProductId = itemDto.ProductId,
                            Quantity = itemDto.Quantity,
                            UnitPrice = price
                        });
                    }
                }

                CalculateOrderTotals(existingOrder, productDetailsCache);
                _unitOfWork.Repository<Order>().Update(existingOrder);
            }
            else
            {
                // Create new order
                var groupedOrderItems = orderDto.OrderItems
                    .GroupBy(item => item.ProductId)
                    .Select(g => new OrderItemDTO
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(item => item.Quantity)
                    })
                    .ToList();

                var orderItems = groupedOrderItems.Select(item =>
                {
                    var (price, _) = productDetailsCache[item.ProductId];
                    return new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price
                    };
                }).ToList();

                existingOrder = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    DiscountPercentage = orderDto.DiscountPercentage,
                    OrderItems = orderItems
                };

                CalculateOrderTotals(existingOrder, productDetailsCache);
                await _unitOfWork.Repository<Order>().AddAsync(existingOrder);
            }

            await _unitOfWork.CompleteAsync();
            return (true, null, _mapper.Map<OrderDTO>(existingOrder));
        }

        public async Task<(bool Success, string ErrorMessage, OrderDTO Order)> UpdateOrderAsync(OrderDTO orderDto)
        {
            var existingOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(orderDto.OrderId,
                include: q => q.Include(o => o.OrderItems));

            if (existingOrder == null)
            {
                return (false, $"Order with ID {orderDto.OrderId} not found.", null);
            }

            // Validate customer
            if (!await _unitOfWork.Repository<Order>().CustomerExistsAsync(orderDto.CustomerId))
            {
                return (false, $"Customer with ID {orderDto.CustomerId} does not exist.", null);
            }

            // Validate all products and cache their details
            var productDetailsCache = new Dictionary<int, (decimal Price, decimal TaxPercentage)>();
            foreach (var item in orderDto.OrderItems)
            {
                try
                {
                    var details = await _unitOfWork.Repository<Order>().GetProductDetailsAsync(item.ProductId);
                    productDetailsCache[item.ProductId] = details;
                }
                catch (ArgumentException)
                {
                    return (false, $"Product with ID {item.ProductId} does not exist.", null);
                }
            }

            // Group and sum quantities for duplicate product IDs
            var groupedOrderItems = orderDto.OrderItems
                .GroupBy(item => item.ProductId)
                .Select(g => new OrderItemDTO
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(item => item.Quantity)
                })
                .ToList();

            // Update order items
            var updatedOrderItems = new List<OrderItem>();
            foreach (var item in groupedOrderItems)
            {
                var (price, _) = productDetailsCache[item.ProductId];
                var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == item.ProductId);

                if (existingItem != null)
                {
                    existingItem.Quantity = item.Quantity;
                    existingItem.UnitPrice = price;
                    updatedOrderItems.Add(existingItem);
                }
                else
                {
                    updatedOrderItems.Add(new OrderItem
                    {
                        OrderId = existingOrder.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price
                    });
                }
            }

            existingOrder.CustomerId = orderDto.CustomerId;
            existingOrder.OrderDate = orderDto.OrderDate;
            existingOrder.DiscountPercentage = orderDto.DiscountPercentage;
            existingOrder.OrderItems = updatedOrderItems;

            CalculateOrderTotals(existingOrder, productDetailsCache);
            _unitOfWork.Repository<Order>().Update(existingOrder);
            await _unitOfWork.CompleteAsync();

            var updatedOrderDto = _mapper.Map<OrderDTO>(existingOrder);
            return (true, null, updatedOrderDto);
        }

       public async Task<Guid> DeleteOrderAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order != null)
            {
                _unitOfWork.Repository<Order>().Remove(order);
                await _unitOfWork.CompleteAsync();
                return id; 
            }
            else
            {
                throw new OrderNotFoundException(id);
            }
        }
        private void CalculateOrderTotals(
            Order order,
            Dictionary<int, (decimal Price, decimal TaxPercentage)> productDetailsCache)
        {
            decimal totalAmount = 0;
            foreach (var item in order.OrderItems)
            {
                var (_, taxPercentage) = productDetailsCache[item.ProductId];
                decimal itemTotal = item.UnitPrice * item.Quantity;
                decimal tax = itemTotal * (taxPercentage / 100);
                totalAmount += itemTotal + tax;
            }

            order.TotalAmount = totalAmount;
            order.DiscountedTotal = totalAmount * (1 - order.DiscountPercentage / 100);
        }
    }
}
