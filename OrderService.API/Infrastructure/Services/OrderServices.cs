using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.KafkaMessageBroker;
using OrderService.API.Infrastructure.RabbitMQMessageBroker;
using OrderService.API.Infrastructure.RedisMessageBroker;
using OrderService.API.Infrastructure.UnitOfWork;
using SharedRepository.Repositories;

namespace OrderService.API.Infrastructure.Services
{
    public class OrderServices : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IMessagePublisher<OrderDTO> _messagePublisher; 
        private readonly ILogger<OrderServices> _logger;
        private readonly RabbitMQSettings _rabbitMQSettings;

        private readonly IKafkaMessagePublisher<OrderDTO> _kafkamessagePublisher;
        private readonly KafkaSettings _kafkaSettings;

        private readonly IRedisMessagePublisher<OrderDTO> _redisMessagePublisher;
        private readonly RedisChannelSettings _redisChannelSettings;

        private readonly string dbconnection = "Host=dpg-ctuh03lds78s73fntmag-a.oregon-postgres.render.com;Database=order_management_db;Username=netconsumer;Password=wv5ZjPAcJY8ICgPJF0PZUV86qdKx2r7d";
        public OrderServices(IUnitOfWork unitOfWork, IDataAccessHelper dataAccessHelper, IMapper mapper, IMessagePublisher<OrderDTO> messagePublisher, ILogger<OrderServices> logger,
          IKafkaMessagePublisher<OrderDTO> kafkamessagePublisher, IRedisMessagePublisher<OrderDTO> redisMessagePublisher,
    IOptions<KafkaSettings> kafkaSettings, IOptions<RabbitMQSettings> rabbitMQSettingsOptions,
    IOptions<RedisChannelSettings> redisChannelSettings)
        {
            _unitOfWork = unitOfWork;
            _dataAccessHelper = dataAccessHelper;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _logger = logger;
            _rabbitMQSettings = rabbitMQSettingsOptions.Value;
            _kafkamessagePublisher = kafkamessagePublisher;
            _kafkaSettings = kafkaSettings.Value;
            _redisMessagePublisher = redisMessagePublisher;
            _redisChannelSettings = redisChannelSettings.Value;
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

            try
            {
                // Publish the OrderCreated event message by using RabbitMQ
                // await _messagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder ?? newOrder), "order_created_queue");
                await _messagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder ?? newOrder), _rabbitMQSettings.Queues.OrderCreated);
                _logger.LogInformation($"OrderCreated event published successfully for the {_rabbitMQSettings.Queues.OrderCreated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderCreated event to RabbitMQ");
                throw;
            }

            // kafka publish message to topic ordercreated for testing after publishing the message in RabbitMQ in the above line of code
            try
            {
                await _kafkamessagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder ?? newOrder), _kafkaSettings.Topics.OrderCreated);
                _logger.LogInformation($"OrderCreated topic published successfully for the {_kafkaSettings.Topics.OrderCreated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderCreated event to Kafka");
                throw;
            }

            // Redis publish message to ordercreated event for testing after publishing the message in RabbitMQ and kafka in the above line of code
            try
            {
                // Publish the OrderCreated event message by using Redis Pub/Sub
                await _redisMessagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder ?? newOrder), _redisChannelSettings.OrderCreatedChannel);
                _logger.LogInformation($"OrderCreated event published successfully for the {_redisChannelSettings.OrderCreatedChannel}.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderCreated event to Redis");
                throw;
            }

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

            try
            {
                // Publish the OrderUpdated event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder), _rabbitMQSettings.Queues.OrderUpdated);
                _logger.LogInformation($"OrderUpdated event published successfully for the {_rabbitMQSettings.Queues.OrderUpdated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderUpdated event to RabbitMQ");
                throw;
            }

            // kafka publish message to topic orderupdated for testing after publishing the message in RabbitMQ in the above line of code
            try
            {
                // Publish the OrderUpdated message by using kafka
                await _kafkamessagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder), _kafkaSettings.Topics.OrderUpdated);
                _logger.LogInformation($"OrderUpdated kafka published successfully for the {_kafkaSettings.Topics.OrderUpdated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderUpdated message to kafka");
                throw;
            }

            // Redis publish message to ordercreated event for testing after publishing the message in RabbitMQ and kafka in the above line of code
            try
            {
                // Publish the OrderUpdated message by using Redis
                await _redisMessagePublisher.PublishAsync(_mapper.Map<OrderDTO>(existingOrder), _redisChannelSettings.OrderUpdatedChannel);
                _logger.LogInformation($"OrderUpdated Redis published successfully for the {_redisChannelSettings.OrderUpdatedChannel}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderUpdated message to kafka");
                throw;
            }

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

            try
            {
                // Publish the OrderDeleted event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<OrderDTO>(order), _rabbitMQSettings.Queues.OrderDeleted);
                _logger.LogInformation($"OrderDeleted event published successfully for the {_rabbitMQSettings.Queues.OrderDeleted}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderDeleted event to RabbitMQ");
                throw;
            }

            // kafka publish message to topic orderdeleted for testing after publishing the message in RabbitMQ in the above line of code
            try
            {
                // Publish the OrderUpdated message by using kafka
                await _kafkamessagePublisher.PublishAsync(_mapper.Map<OrderDTO>(order), _kafkaSettings.Topics.OrderDeleted);
                _logger.LogInformation($"OrderDeleted kafka published successfully for the {_kafkaSettings.Topics.OrderDeleted}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderDeleted message to kafka");
                throw;
            }

            // Redis publish message to ordercreated event for testing after publishing the message in RabbitMQ and kafka in the above line of code
            try
            {
                // Publish the OrderUpdated message by using Redis
                await _redisMessagePublisher.PublishAsync(_mapper.Map<OrderDTO>(order), _redisChannelSettings.OrderDeletedChannel);
                _logger.LogInformation($"OrderDeleted kafka published successfully for the {_redisChannelSettings.OrderDeletedChannel}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing OrderDeleted message to Redis");
                throw;
            }

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
