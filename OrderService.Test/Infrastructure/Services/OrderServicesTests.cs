using AutoMapper;
using FluentAssertions;
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
            result.Should().BeEquivalentTo(cachedOrders);
            _mockUnitOfWork.Verify(x => x.Repository<Order>(), Times.Never());
        }

        [Fact]
        public async Task GetAllOrdersAsync_CacheMiss_ReturnsOrdersFromDatabase()
        {
            // Arrange
            var orders = new List<Order> { new Order { OrderId = Guid.NewGuid() } };
            IEnumerable<OrderDTO> orderDtos = new List<OrderDTO> { new OrderDTO { OrderId = orders[0].OrderId } }; 
            var mockRepo = new Mock<IGenericRepository<Order>>();
            _mockCacheService.Setup(x => x.GetAsync<IEnumerable<OrderDTO>>("orders:all")).ReturnsAsync((IEnumerable<OrderDTO>)null);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.GetAllAsync(null, null, It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                    .ReturnsAsync(orders);
            _mockMapper.Setup(x => x.Map<IEnumerable<OrderDTO>>(orders)).Returns(orderDtos);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            result.Should().BeEquivalentTo(orderDtos);
            _mockCacheService.Verify(x => x.SetAsync("orders:all", It.Is<IEnumerable<OrderDTO>>
                                    (d => d.SequenceEqual(orderDtos)), TimeSpan.FromMinutes(5)), Times.Once());
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
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { order = cachedOrder });
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
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { message = $"Order with ID {orderId} not found." });
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

            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId))
                .ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId))
                .ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetProductDetailsAsync(1))
                .ReturnsAsync(new ProductDetailsResponse
                {
                    ProductId = 1,
                    Price = 10f,
                    TaxPercentage = 5f,
                    Stock = 10
                });
            _mockDataAccessHelper.Setup(x => x.UpdateProductStockByOrderedAsync(1, -1))
                .ReturnsAsync(true);
            _mockUnitOfWork.Setup(x => x.Repository<Order>()).Returns(mockRepo.Object);
            mockRepo.Setup(x => x.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _mockMapper.Setup(x => x.Map<OrderDTO>(It.IsAny<Order>())).Returns(orderDto);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task AddOrderAsync_CustomerNotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO { CustomerId = Guid.NewGuid() };
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(false);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().BeEquivalentTo(new 
            { 
                message = $"Customer with ID {orderDto.CustomerId} does not exist." });
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
            _mockDataAccessHelper.Setup(x => x.ExistsAsync("customers", "customer_id", orderDto.CustomerId)).ReturnsAsync(true);
            _mockDataAccessHelper.Setup(x => x.GetInactiveCustomerFlag(orderDto.CustomerId)).ReturnsAsync(false);

            // Act
            var result = await _orderService.AddOrderAsync(orderDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().BeEquivalentTo(new 
            { 
                message = $"Customer with ID {orderDto.CustomerId} is not in active state." });
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
            result.Should().BeOfType<OkObjectResult>();
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
            result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().BeEquivalentTo(new 
            { 
                message = $"Order with ID {orderDto.OrderId} does not exist." });
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
            result.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { message = "Order deleted successfully." });
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
            result.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new { message = $"Order with ID {orderId} not found." });
        }
    }
}

    
