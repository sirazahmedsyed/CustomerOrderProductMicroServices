using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.UnitOfWork;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.Repositories;
using SharedRepository.RedisCache;

namespace OrderService.API.Infrastructure.Services
{
    public class OrderServices : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly ICacheService _cacheService;
        private readonly ILogger<OrderServices> _logger;

        private const string ALL_ORDERS_KEY = "orders:all";
        private const string ORDER_KEY_PREFIX = "order:";
        private const string CUSTOMER_ORDERS_PREFIX = "customer:orders:";

        public OrderServices(IUnitOfWork unitOfWork, IDataAccessHelper dataAccessHelper, IMapper mapper,
            IHttpContextAccessor httpContextAccessor, IRabbitMQHelper rabbitMQHelper, ICacheService cacheService, ILogger<OrderServices> logger)
        {
            _unitOfWork = unitOfWork;
            _dataAccessHelper = dataAccessHelper;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _rabbitMQHelper = rabbitMQHelper;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            var cachedOrders = await _cacheService.GetAsync<IEnumerable<OrderDTO>>(ALL_ORDERS_KEY);
            if (cachedOrders != null)
            {
                _logger.LogInformation("Retrieved orders from cache");
                return cachedOrders;
            }

            var orders = await _unitOfWork.Repository<Order>().GetAllAsync(include: q => q.Include(o => o.OrderItems));

            await _cacheService.SetAsync(ALL_ORDERS_KEY, _mapper.Map<IEnumerable<OrderDTO>>(orders), TimeSpan.FromMinutes(5));

            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task<IActionResult> GetOrderByIdAsync(Guid id)
        {
            var cacheKey = $"{ORDER_KEY_PREFIX}{id}";

            var cachedOrder = await _cacheService.GetAsync<OrderDTO>(cacheKey);
            if (cachedOrder != null)
            {
                _logger.LogInformation($"Retrieved order {id} from cache");
                return new OkObjectResult(new { order = cachedOrder });
            }

            var order = await _unitOfWork.Repository<Order>().GetByIdAsync(id, include: q => q.Include(o => o.OrderItems));

            if (order == null)
            {
                return new BadRequestObjectResult(new { message = $"Order with ID {id} not found." });
            }

            await _cacheService.SetAsync(cacheKey, _mapper.Map<OrderDTO>(order), TimeSpan.FromMinutes(30));

            return new OkObjectResult(new { order = _mapper.Map<OrderDTO>(order) });
        }

        public async Task<IActionResult> AddOrderAsync(OrderDTO orderDto)
        {
            var result = await ProcessOrderCreation(orderDto);
            if (result is BadRequestObjectResult)
            {
                return result;
            }

            await InvalidateOrderCaches(orderDto.CustomerId);

            return result;
        }

        public async Task<IActionResult> UpdateOrderAsync(OrderDTO orderDto)
        {
            var result = await ProcessOrderUpdate(orderDto);
            if (result is BadRequestObjectResult)
            {
                return result;
            }

            await InvalidateOrderCaches(orderDto.CustomerId);

            return result;
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

            await InvalidateOrderCaches(order.CustomerId);

            await SendAuditMessage(3, id, "Deleted");

            return new OkObjectResult(new { message = "Order deleted successfully." });
        }

        private async Task InvalidateOrderCaches(Guid customerId)
        {
            await _cacheService.RemoveAsync(ALL_ORDERS_KEY);
            await _cacheService.RemoveByPatternAsync($"{CUSTOMER_ORDERS_PREFIX}{customerId}:*");

            _logger.LogInformation($"Invalidated caches for customer {customerId}");
        }

        private async Task SendAuditMessage(int operationType, Guid orderId, string action)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var auditMessageDto = new AuditMessageDto
            {
                OprtnTyp = operationType,
                UsrNm = username,
                UsrNo = 1,
                LogDsc = new List<string> { $"{action} By {username} {DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss 'UTC' yyyy")}" },
                LogTyp = 1,
                LogDate = DateTime.UtcNow,
                ScreenName = "OrdersController",
                ObjectName = "order",
                ScreenPk = orderId
            };
            await _rabbitMQHelper.AuditResAsync(auditMessageDto);
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

            _logger.LogInformation($"Calculated totals for order {order.OrderId}: Total={totalAmount}, Discounted={order.DiscountedTotal}");
        }

        private async Task<IActionResult> ProcessOrderCreation(OrderDTO orderDto)
        {
            try
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
                        return new BadRequestObjectResult(new
                        {
                            message = $"Insufficient stock for product ID {item.ProductId}. Available: {availableStock}, Requested: {item.Quantity}"
                        });
                    }
                }

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

                Order newOrder = new Order
                {
                    OrderId = Guid.NewGuid(),
                    CustomerId = orderDto.CustomerId,
                    OrderDate = DateTime.UtcNow,
                    DiscountPercentage = orderDto.DiscountPercentage,
                    OrderItems = orderItems
                };

                CalculateOrderTotals(newOrder, productDetailsCache);
                await _unitOfWork.Repository<Order>().AddAsync(newOrder);

                foreach (var orderitem in orderDto.OrderItems)
                {
                    var stockUpdated = await _dataAccessHelper.UpdateProductStockByOrderedAsync(orderitem.ProductId, -orderitem.Quantity);
                    if (!stockUpdated)
                    {
                        return new BadRequestObjectResult(new { message = $"Failed to update stock for product ID {orderitem.ProductId}." });
                    }
                }

                await _unitOfWork.CompleteAsync();

                await SendAuditMessage(1, newOrder.OrderId, "Created");

                _logger.LogInformation($"Created new order with ID {newOrder.OrderId}");
                return new OkObjectResult(_mapper.Map<OrderDTO>(newOrder));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order creation");
                return new BadRequestObjectResult(new { message = "Error processing order creation" });
            }
        }

        private async Task<IActionResult> ProcessOrderUpdate(OrderDTO orderDto)
        {
            try
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

                await SendAuditMessage(2, existingOrder.OrderId, "Updated");

                _logger.LogInformation($"Updated order with ID {existingOrder.OrderId}");
                return new OkObjectResult(new
                {
                    message = "Order updated successfully.",
                    order = _mapper.Map<OrderDTO>(existingOrder)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order update");
                return new BadRequestObjectResult(new { message = "Error processing order update" });
            }
        }
    }
}