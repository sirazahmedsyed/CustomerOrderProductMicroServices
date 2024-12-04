using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.UnitOfWork;
using SharedRepository.Repositories;

namespace OrderService.API.Infrastructure.Services
{
    public class OrderServices : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly string dbconnection = "Host=dpg-csl1qfrv2p9s73ae0iag-a.oregon-postgres.render.com;Database=inventorymanagement_h8uy;Username=netconsumer;Password=UBmEj8MjJqg4zlimlXovbyt0bBDcrmiF";
        public OrderServices(IUnitOfWork unitOfWork, IDataAccessHelper dataAccessHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _dataAccessHelper = dataAccessHelper;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(
                include: q => q.Include(o => o.OrderItems));
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<IActionResult> GetOrderByIdAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id,
                include: q => q.Include(o => o.OrderItems));

            if (order == null)
            {
                return new BadRequestObjectResult(new { message = $"Order with ID {id} not found." });
            }

            return new OkObjectResult(new
            {
                order = _mapper.Map<OrderDTO>(order)
            });
        }

        public async Task<IActionResult> AddOrderAsync(OrderDTO orderDto)
        {
            if (!await _dataAccessHelper.ExistsAsync("customers", "customer_id", orderDto.CustomerId))
            {
                return new BadRequestObjectResult(new { message = $"Customer with ID {orderDto.CustomerId} does not exist." });
            }

            if (!await _dataAccessHelper.GetInactiveCustomerFlag(orderDto.CustomerId))
            {
                return new BadRequestObjectResult(new { message = $"Customer with ID {orderDto.CustomerId} is not in active state." });
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
                    var productDetails = await _dataAccessHelper.GetProductDetailsAsync(item.ProductId);

                    if (productDetails.ProductId == default)
                    {
                        return new BadRequestObjectResult(new { message = $"Product with ID {item.ProductId} does not exist." });
                    }

                    productDetailsCache[item.ProductId] = (Convert.ToDecimal(productDetails.Price), Convert.ToDecimal(productDetails.TaxPercentage));
                    productStockCache[item.ProductId] = productDetails.Stock;
                }

                var availableStock = productStockCache[item.ProductId];
                if (availableStock < item.Quantity)
                {
                    return new BadRequestObjectResult(new { message = $"Insufficient stock for product ID {item.ProductId}. Available: {availableStock}, Requested: {item.Quantity}" });
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

            foreach (var orderitem in orderDto.OrderItems)
            {
                var stockUpdated = await _dataAccessHelper.UpdateProductStockByOrderedAsync(orderitem.ProductId, -orderitem.Quantity);
                if (!stockUpdated)
                {
                    return new BadRequestObjectResult(new { message = $"Failed to update stock for product ID {orderitem.ProductId}." });
                }
            }

            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(_mapper.Map<OrderDTO>(existingOrder ?? newOrder));
        }

        public async Task<IActionResult> UpdateOrderAsync(OrderDTO orderDto)
        {
            if (!await _dataAccessHelper.ExistsAsync("orders", "order_id", orderDto.OrderId))
            {
                return new BadRequestObjectResult(new { message = $"Order with ID {orderDto.OrderId} does not exist." });
            }


            if (!await _dataAccessHelper.ExistsAsync("customers", "customer_id", orderDto.CustomerId))
            {
                return new BadRequestObjectResult(new { message = $"Customer with ID {orderDto.CustomerId} does not exist." });
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
                    var productDetails = await _dataAccessHelper.GetProductDetailsAsync(item.ProductId);

                    if (productDetails.ProductId == 0)
                    {
                        return new BadRequestObjectResult(new { message = $"Product with ID {item.ProductId} does not exist." });
                    }
                    productDetailsCache[item.ProductId] = (Convert.ToDecimal(productDetails.Price), Convert.ToDecimal(productDetails.TaxPercentage));
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
                    return new BadRequestObjectResult(new { message = $"Insufficient stock for product ID {itemDto.ProductId}. Available stock: {availableStock}." });
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
                await _dataAccessHelper.UpdateProductStockAsync(itemDto.ProductId, -itemDto.Quantity);
            }

            CalculateOrderTotals(existingOrder, productDetailsCache);
            _unitOfWork.Repository<Order>().Update(existingOrder);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new 
            { 
                message = "Order updated successfully.", 
                order = _mapper.Map<OrderDTO>(existingOrder) 
            });
        }

        public async Task<IActionResult> DeleteOrderAsync(Guid id)
        {
            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id);
            if (order == null)
            {
                return new BadRequestObjectResult(new { message = $"Order with ID {id} not found." });
                
            }
            _unitOfWork.Repository<Order>().Remove(order);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new { message = "Order deleted successfully." });
        }

        private void CalculateOrderTotals(Order order, Dictionary<int, (decimal Price, decimal TaxPercentage)> productDetailsCache)
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
