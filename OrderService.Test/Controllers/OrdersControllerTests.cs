using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.API.Controllers;
using OrderService.API.Infrastructure.DTOs;
using OrderService.API.Infrastructure.Services;

namespace OrderService.Test.Controllers
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrdersController>>();
            _controller = new OrdersController(_mockOrderService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllOrders_Successful_ReturnsOkResult()
        {
            // Arrange
            var orders = new List<OrderDTO> { new OrderDTO { OrderId = Guid.NewGuid() } };
            _mockOrderService.Setup(x => x.GetAllOrdersAsync()).ReturnsAsync(orders);

            // Act
            var result = await _controller.GetAllOrders();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(orders, okResult.Value);
        }

        [Fact]
        public async Task GetAllOrders_Exception_ReturnsInternalServerError()
        {
            // Arrange
            _mockOrderService.Setup(x => x.GetAllOrdersAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetAllOrders();

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal server error", objectResult.Value);
        }

        [Fact]
        public async Task GetOrderById_Successful_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var orderDto = new OrderDTO { OrderId = orderId };
            _mockOrderService.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(new OkObjectResult(orderDto));

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(orderDto, okResult.Value);
        }

        [Fact]
        public async Task GetOrderById_NotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var badRequestResult = new BadRequestObjectResult(new { message = "Order not found" });
            _mockOrderService.Setup(x => x.GetOrderByIdAsync(orderId)).ReturnsAsync(badRequestResult);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequest = (BadRequestObjectResult)result;
            var value = badRequest.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Order not found", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task GetOrderById_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(x => x.GetOrderByIdAsync(orderId)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal server error", objectResult.Value);
        }

        [Fact]
        public async Task CreateOrder_ValidModel_ReturnsSuccessResult()
        {
            // Arrange
            var orderDto = new OrderDTO { OrderId = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
            var okResult = new OkObjectResult(orderDto);
            _mockOrderService.Setup(x => x.AddOrderAsync(orderDto)).ReturnsAsync(okResult);
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.CreateOrder(orderDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResultActual = (OkObjectResult)result;
            Assert.Equal(orderDto, okResultActual.Value);
        }

        [Fact]
        public async Task CreateOrder_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO();
            _controller.ModelState.AddModelError("CustomerId", "Required");

            // Act
            var result = await _controller.CreateOrder(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            //_mockLogger.Verify(x => x.Log(
            //    LogLevel.Warning,
            //    It.IsAny<EventId>(),
            //    It.IsAny<object>(),
            //    null,
            //    It.IsAny<Func<object, Exception?, string>>()), Times.Once());
        }

        [Fact]
        public async Task CreateOrder_ServiceBadRequest_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO { OrderId = Guid.NewGuid() };
            var badRequestResult = new BadRequestObjectResult(new { message = "Invalid order" });
            _mockOrderService.Setup(x => x.AddOrderAsync(orderDto)).ReturnsAsync(badRequestResult);
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.CreateOrder(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequest = (BadRequestObjectResult)result;
            var value = badRequest.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Invalid order", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task UpdateOrder_ValidModel_ReturnsSuccessResult()
        {
            // Arrange
            var orderDto = new OrderDTO { OrderId = Guid.NewGuid(), CustomerId = Guid.NewGuid() };
            var okResult = new OkObjectResult(new { message = "Order updated" });
            _mockOrderService.Setup(x => x.UpdateOrderAsync(orderDto)).ReturnsAsync(okResult);
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.UpdateOrder(orderDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResultActual = (OkObjectResult)result;
            var value = okResultActual.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Order updated", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task UpdateOrder_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var orderDto = new OrderDTO();
            _controller.ModelState.AddModelError("CustomerId", "Required");

            // Act
            var result = await _controller.UpdateOrder(orderDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_Successful_ReturnsOkResult()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var okResult = new OkObjectResult(new { message = "Order deleted successfully" });
            _mockOrderService.Setup(x => x.DeleteOrderAsync(orderId)).ReturnsAsync(okResult);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResultActual = (OkObjectResult)result;
            var value = okResultActual.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Order deleted successfully", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task DeleteOrder_NotFound_ReturnsBadRequest()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var badRequestResult = new BadRequestObjectResult(new { message = "Order not found" });
            _mockOrderService.Setup(x => x.DeleteOrderAsync(orderId)).ReturnsAsync(badRequestResult);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequest = (BadRequestObjectResult)result;
            var value = badRequest.Value;
            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("Order not found", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task DeleteOrder_Exception_ReturnsInternalServerError()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _mockOrderService.Setup(x => x.DeleteOrderAsync(orderId)).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<ObjectResult>(result);
            var objectResult = (ObjectResult)result;
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Internal server error", objectResult.Value);
        }
    }
}
