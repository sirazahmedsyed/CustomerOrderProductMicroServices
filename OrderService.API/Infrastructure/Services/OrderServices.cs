using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
using SharedRepository.Repositories;
using System.Data.Common;
namespace OrderService.API.Infrastructure.Services
{
public class OrderServices : IOrderService
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICusotmerHelper _customerHelper;
        private readonly IProductHelper _productHelper;
        private readonly IOrderHelper _orderHelper;
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
        public OrderServices(IUnitOfWork unitOfWork, ICusotmerHelper customerHelper, IProductHelper productHelper, IOrderHelper orderHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _customerHelper = customerHelper;
            _productHelper = productHelper;
            _orderHelper = orderHelper;
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
            if (!await _customerHelper.CustomerExistsAsync(orderDto.CustomerId))
            {
                return (false, $"Customer with ID {orderDto.CustomerId} does not exist.", null);
            }

            var groupedOrderItems = orderDto.OrderItems
                .GroupBy(item => item.ProductId)
                .Select(g => new OrderItemDTO
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(item => item.Quantity)
                })
                .ToList();

            var productDetailsCache = new Dictionary<int, (decimal Price, decimal TaxPercentage)>();
            var productStockCache = new Dictionary<int, int>();
            foreach (var item in groupedOrderItems)
            {
                if (!productDetailsCache.ContainsKey(item.ProductId))
                {
                    var productDetails = await _productHelper.GetProductDetailsAsync(item.ProductId);
                    if (productDetails.ProductId == null)
                    {
                        return (false, $"Product with ID {item.ProductId} does not exist.", null);
                    }

                    productDetailsCache[item.ProductId] = (productDetails.Price, productDetails.TaxPercentage);
                    productStockCache[item.ProductId] = productDetails.Stock;
                }

                var availableStock = productStockCache[item.ProductId];
                if (availableStock < item.Quantity)
                {
                    return (false, $"Insufficient stock for product ID {item.ProductId}. Available: {availableStock}, Requested: {item.Quantity}", null);
                }
            }

            var existingOrder = (await _unitOfWork.Repository<Order>()
                .GetAllAsync(
                    o => o.CustomerId == orderDto.CustomerId &&
                    o.OrderItems.Any(oi => groupedOrderItems.Select(i => i.ProductId).Contains(oi.ProductId)),
                    include: q => q.Include(o => o.OrderItems)))
                .FirstOrDefault();

            Order newOrder = null;
            if (existingOrder != null)
            {
                foreach (var itemDto in groupedOrderItems)
                {
                    var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);
                    var (price, tax) = productDetailsCache[itemDto.ProductId];

                    if (existingItem != null)
                    {
                        existingItem.Quantity += itemDto.Quantity;
                    }
                    else
                    {
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
                var orderItems = groupedOrderItems.Select(item =>
                {
                    var (price, tax) = productDetailsCache[item.ProductId];
                    return new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = price
                    };
                }).ToList();

                 newOrder = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    DiscountPercentage = orderDto.DiscountPercentage,
                    OrderItems = orderItems
                };

                CalculateOrderTotals(newOrder, productDetailsCache);
                await _unitOfWork.Repository<Order>().AddAsync(newOrder);
            }

            foreach (var item in orderDto.OrderItems)
            {
                var stockUpdated = await _productHelper.UpdateProductStockByOrderedAsync(item.ProductId, -item.Quantity); 
                if (!stockUpdated)
                {
                    return (false, $"Failed to update stock for product ID {item.ProductId}.", null);
                }
            }
            await _unitOfWork.CompleteAsync();
            return (true, null, _mapper.Map<OrderDTO>(existingOrder ?? newOrder));
        }

        public async Task<(bool Success, string ErrorMessage, OrderDTO Order)> UpdateOrderAsync(OrderDTO orderDto)
        {
            if (!await _orderHelper.OrderExistsAsync(orderDto.OrderId))
            {
                return (false, $"Order with ID {orderDto.OrderId} does not exist.", null);
            }

            if (!await _customerHelper.CustomerExistsAsync(orderDto.CustomerId))
            {
                return (false, $"Customer with ID {orderDto.CustomerId} does not exist.", null);
            }

            var groupedOrderItems = orderDto.OrderItems
                .GroupBy(item => item.ProductId)
                .Select(g => new OrderItemDTO
                {
                    ProductId = g.Key,
                    Quantity = g.Sum(item => item.Quantity)
                })
                .ToList();

            var productDetailsCache = new Dictionary<int, (decimal Price, decimal TaxPercentage)>();
            var productStockCache = new Dictionary<int, int>();

            foreach (var item in groupedOrderItems)
            {
                if (!productDetailsCache.ContainsKey(item.ProductId))
                {
                    var productDetails = await _productHelper.GetProductDetailsAsync(item.ProductId);
                    if (productDetails.ProductId == null)
                    {
                        return (false, $"Product with ID {item.ProductId} does not exist.", null);
                    }
                    productDetailsCache[item.ProductId] = (productDetails.Price, productDetails.TaxPercentage);
                    productStockCache[item.ProductId] = productDetails.Stock;
                }
            }
            
            var existingOrder = await _unitOfWork.Repository<Order>().GetByIdAsync(orderDto.OrderId, o => o.Include(oi => oi.OrderItems));
            existingOrder.OrderDate = DateTime.UtcNow;
            existingOrder.DiscountPercentage = orderDto.DiscountPercentage;

            foreach (var itemDto in groupedOrderItems)
            {
                var (price, taxPercentage) = productDetailsCache[itemDto.ProductId];
                var existingItem = existingOrder.OrderItems.FirstOrDefault(oi => oi.ProductId == itemDto.ProductId);
                int totalQuantity = itemDto.Quantity;
                if (existingItem != null)
                {
                    totalQuantity += existingItem.Quantity;
                }

                var availableStock = productStockCache[itemDto.ProductId];
                if (totalQuantity > availableStock)
                {
                    return (false, $"Insufficient stock for product ID {itemDto.ProductId}. Available stock: {availableStock}.", null);
                }

                if (existingItem != null)
                {
                    existingItem.Quantity = totalQuantity;
                    existingItem.UnitPrice = price; 
                }
                else
                {
                    existingOrder.OrderItems.Add(new OrderItem
                    {
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = price,
                    });
                }
                await _productHelper.UpdateProductStockAsync(itemDto.ProductId, -itemDto.Quantity);
            }

            CalculateOrderTotals(existingOrder, productDetailsCache);
            _unitOfWork.Repository<Order>().Update(existingOrder);
            await _unitOfWork.CompleteAsync();
            return (true, null, _mapper.Map<OrderDTO>(existingOrder));
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
