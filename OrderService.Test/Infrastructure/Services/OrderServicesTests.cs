using AutoMapper;
using GrpcService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Entities;
using OrderService.API.Infrastructure.Services;
using OrderService.API.Infrastructure.UnitOfWork;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.RedisCache;
using SharedRepository.Repositories;

namespace OrderService.Test.Infrastructure.Services
{
    public class OrderServicesTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IDataAccessHelper> _mockDataAccessHelper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IRabbitMQHelper> _mockRabbitMQHelper;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<ILogger<OrderServices>> _mockLogger;
        private readonly OrderServices _orderService;

        public OrderServicesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockDataAccessHelper = new Mock<IDataAccessHelper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockRabbitMQHelper = new Mock<IRabbitMQHelper>();
            _mockCacheService = new Mock<ICacheService>();
            _mockLogger = new Mock<ILogger<OrderServices>>();

            _orderService = new OrderServices(
                _mockUnitOfWork.Object,
                _mockDataAccessHelper.Object,
                _mockMapper.Object,
                _mockHttpContextAccessor.Object,
                _mockRabbitMQHelper.Object,
                _mockCacheService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllOrdersAsync_CacheHit_ReturnsCachedOrders()
        {
            // Arrange
            var cachedOrders = new List<OrderDTO> { new OrderDTO { OrderId = Guid.NewGuid() } };
            _mockCacheService.Setup(x => x.GetAsync<IEnumerable<OrderDTO>>("orders:all"))
                .ReturnsAsync(cachedOrders);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.Equal(cachedOrders, result);
            _mockUnitOfWork.Verify(x => x.Repository<Order>(), Times.Never());
        }

        [Fact]
        public async Task GetAllOrdersAsync_CacheMiss_ReturnsOrdersFromDatabase()
        {
            // Arrange
            var orders = new List<Order> { new Order { OrderId = Guid.NewGuid() } };
            var orderDtos = new List<OrderDTO> { new OrderDTO { OrderId = orders[0].OrderId } };
            var mockRepo = new Mock<IGenericRepository<Order>>();
            _mockCacheService.Setup(x => x.GetAsync<IEnumerable<OrderDTO>>("orders:all")).ReturnsAsync((IEnumerable<OrderDTO>)null);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetAllAsync(null, null, It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                    .ReturnsAsync(orders);
            _mockMapper.Setup(x => x.Map<IEnumerable<OrderDTO>>(orders)).Returns(orderDtos);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.Equal(orderDtos, result);
            _mockCacheService.Verify(x => x.SetAsync("orders:all",
                It.Is<IEnumerable<OrderDTO>>(d => d.SequenceEqual(orderDtos)),
                TimeSpan.FromMinutes(5)), Times.Once());
        }

        [Fact]
        public async Task GetOrderByIdAsync_CacheHit_ReturnsCachedOrder()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var cachedOrder = new OrderDTO { OrderId = orderId };
            _mockCacheService.Setup(x => x.GetAsync<OrderDTO>($"order:{orderId}"))
                .ReturnsAsync(cachedOrder);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            var value = okResult.Value;
            var orderProperty = value.GetType().GetProperty("order");
            Assert.NotNull(orderProperty);
            Assert.Equal(cachedOrder, orderProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>(), Times.Never());
        }

        [Fact]
        public async Task GetOrderByIdAsync_OrderNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var mockRepo = new Mock<IGenericRepository<Order>>();
            _mockCacheService.Setup(x => x.GetAsync<OrderDTO>($"order:{orderId}")).ReturnsAsync((OrderDTO)null);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                    .ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Order with ID {orderId} not found.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task AddOrderAsync_ValidOrder_ReturnsOkResult()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            var order = new Order { OrderId = Guid.NewGuid(), CustomerId = orderDto.CustomerId };
            var mockRepo = new Mock<IGenericRepository<Order>>();

            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse
                {
                    ProductId = 1,
                    Price = 10f,
                    TaxPercentage = 5f,
                    Stock = 10
                });
            _mockDataAccessHelper.Setup(x => x.UpdateProductStockByOrderedAsync(1, -1)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(x => x.Map<OrderDTO>(It.IsAny<Order>())).Returns(orderDto);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task AddOrderAsync_CustomerNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO { CustomerId = Guid.NewGuid() };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId))
                .ReturnsAsync(false);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Customer with ID {orderDto.CustomerId} does not exist.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task AddOrderAsync_InactiveCustomer_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId))
                .ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId))
                .ReturnsAsync(false);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Customer with ID {orderDto.CustomerId} is not in active state.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task UpdateOrderAsync_ValidUpdate_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDTO
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            var existingOrder = new Order { OrderId = orderId, OrderItems = new List<OrderItem>(), CustomerId = orderDto.CustomerId };
            var mockRepo = new Mock<IGenericRepository<Order>>();

            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse
                {
                    ProductId = 1,
                    Price = 10f,
                    TaxPercentage = 5f,
                    Stock = 10
                });
            _mockDataAccessHelper.Setup(x => x.UpdateProductStockAsync(1, -1)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .ReturnsAsync(existingOrder);
            _mockMapper.Setup(x => x.Map<OrderDTO>(existingOrder)).Returns(orderDto);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task UpdateOrderAsync_OrderNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO { OrderId = Guid.NewGuid() };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderDto.OrderId))
                .ReturnsAsync(false);

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Order with ID {orderDto.OrderId} does not exist.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task DeleteOrderAsync_ValidOrder_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order { OrderId = orderId, CustomerId = Guid.NewGuid() };
            var mockRepo = new Mock<IGenericRepository<Order>>();
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(order);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _orderService.DeleteOrderAsync(orderId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            var value = okResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Order deleted successfully.", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteOrderAsync_OrderNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var mockRepo = new Mock<IGenericRepository<Order>>();
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync((Order)null);

            // Act
            var result = await _orderService.DeleteOrderAsync(orderId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Order with ID {orderId} not found.", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task AddOrderAsync_InsufficientStock_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 5 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse
                {
                    ProductId = 1,
                    Stock = 2, // Less than requested quantity (5)
                    Price = 10f,
                    TaxPercentage = 5f
                });

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Insufficient stock for product ID 1. Available: 2, Requested: 5", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>().AddAsync(It.IsAny<Order>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task AddOrderAsync_ProductNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1)).ReturnsAsync(new ProductDetailsResponse { ProductId = 0 }); // ProductId == default (0)

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Product with ID 1 does not exist.", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>().AddAsync(It.IsAny<Order>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task AddOrderAsync_StockUpdateFailed_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            var order = new Order { OrderId = Guid.NewGuid(), CustomerId = orderDto.CustomerId };
            var mockRepo = new Mock<IGenericRepository<Order>>();

            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                                 .ReturnsAsync(new ProductDetailsResponse
                                 {
                                    ProductId = 1,
                                    Stock = 10,
                                    Price = 10f,
                                    TaxPercentage = 5f
                                 });
            _mockDataAccessHelper.Setup(x => x.UpdateProductStockByOrderedAsync(1, -1)).ReturnsAsync(false); // Stock update fails
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Callback<Order>(o => order = o).Returns(Task.CompletedTask);
            _mockMapper.Setup(x => x.Map<Order>(orderDto)).Returns(order);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Failed to update stock for product ID 1.", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task AddOrderAsync_ExceptionThrown_ReturnsBadRequestWithError()
        {
            // Arrange
            var orderDto = new OrderDTO
            {
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1)).ThrowsAsync(new Exception("Database error")); // Exception thrown

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Error processing order creation", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>().AddAsync(It.IsAny<Order>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task UpdateOrderAsync_CustomerNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDTO
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(false); // Customer does not exist

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Customer with ID {orderDto.CustomerId} does not exist.", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>().Update(It.IsAny<Order>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task UpdateOrderAsync_ProductNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDTO
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse { ProductId = 0 }); // ProductId == 0 (default)

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Product with ID 1 does not exist.", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>().Update(It.IsAny<Order>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task UpdateOrderAsync_InsufficientStockWithExistingItem_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var orderDto = new OrderDTO
            {
                OrderId = orderId,
                CustomerId = customerId,
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 3 } }
            };
            var existingOrder = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                OrderItems = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10m } } // Existing quantity: 2
            };
            var mockRepo = new Mock<IGenericRepository<Order>>();

            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse
                {
                    ProductId = 1,
                    Stock = 4, // Total available stock: 4, but totalQuantity = 2 (existing) + 3 (new) = 5
                    Price = 10f,
                    TaxPercentage = 5f
                });
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                    .ReturnsAsync(existingOrder);

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal($"Insufficient stock for product ID 1. Available stock: 4.", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }

        [Fact]
        public async Task UpdateOrderAsync_UpdatesExistingItem_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            var orderDto = new OrderDTO
            {
                OrderId = orderId,
                CustomerId = customerId,
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 3 } }
            };
            var existingOrder = new Order
            {
                OrderId = orderId,
                CustomerId = customerId,
                OrderItems = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10m } } // Existing quantity: 2
            };
            var mockRepo = new Mock<IGenericRepository<Order>>();

            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse
                {
                    ProductId = 1,
                    Stock = 10, // Enough stock for totalQuantity = 5 (2 existing + 3 new)
                    Price = 15f, // Updated price
                    TaxPercentage = 5f
                });
            _mockDataAccessHelper.Setup(x => x.UpdateProductStockAsync(1, -3)).ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetByIdAsync(orderId, It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                    .ReturnsAsync(existingOrder);
            _mockMapper.Setup(x => x.Map<OrderDTO>(existingOrder)).Returns(orderDto);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
            _mockRabbitMQHelper.Setup(x => x.AuditResAsync(It.IsAny<AuditMessageDto>())).ReturnsAsync(true);

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            var value = okResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Order updated successfully.", messageProperty.GetValue(value));
            var updatedItem = existingOrder.OrderItems.First(); // Use First() instead of indexing
            Assert.Equal(5, updatedItem.Quantity); // Updated quantity: 2 + 3
            Assert.Equal(15m, updatedItem.UnitPrice); // Updated price
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task UpdateOrderAsync_ExceptionThrown_ReturnsBadRequestWithError()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDTO
            {
                OrderId = orderId,
                CustomerId = Guid.NewGuid(),
                OrderItems = new List<OrderItemDTO> { new OrderItemDTO { ProductId = 1, Quantity = 1 } }
            };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("orders", "order_id", orderId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ThrowsAsync(new Exception("Database error")); // Exception thrown

            // Act
            var result = await _orderService.UpdateOrderAsync(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Error processing order update", messageProperty.GetValue(value));
            _mockUnitOfWork.Verify(x => x.Repository<Order>().Update(It.IsAny<Order>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
        }
    }
}

    
