using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.API.Controllers;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Services;

namespace ProductService.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly ProductsController _controller;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_mockProductService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CreateProduct_ValidProduct_ReturnsOkResult()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "New Product" };
            var expectedMessage = new { message = "Product created successfully", product = productDto };
            _mockProductService.Setup(x => x.AddProductAsync(productDto)).ReturnsAsync(new OkObjectResult(expectedMessage));
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.CreateProduct(productDto);

            // Assert
            Assert.IsType<OkObjectResult>(result); // Type check
            var okObjectResult = (OkObjectResult)result; // Cast result
            Assert.Equal(expectedMessage, okObjectResult.Value); // Value check
            _mockProductService.Verify(x => x.AddProductAsync(productDto), Times.Once());
            _mockLogger.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never());
        }

        [Fact] // Added: Missing attribute to mark as a test case
        public async Task CreateProduct_ServiceThrowsException_Returns500()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "New Product" };
            var exception = new Exception("Service error");

            _mockProductService.Setup(x => x.AddProductAsync(productDto)).ThrowsAsync(exception);
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.CreateProduct(productDto);

            // Assert
            Assert.IsType<ObjectResult>(result); // Changed: Replaced FluentAssertions with xUnit assertion for type check
            var statusCodeResult = (ObjectResult)result; // Changed: Replaced Subject with direct cast
            Assert.Equal(500, statusCodeResult.StatusCode); // Changed: Replaced FluentAssertions with xUnit assertion for status code check
            Assert.Equal("Internal server error", statusCodeResult.Value); // Changed: Replaced FluentAssertions with xUnit assertion for value check
            _mockProductService.Verify(x => x.AddProductAsync(productDto), Times.Once());
            //_mockLogger.Verify(x => x.Log(
            //    LogLevel.Error,
            //    It.IsAny<EventId>(),
            //    It.Is<object>(o => o.ToString().Contains($"Error occurred while creating product with ID {productDto.ProductId}")),
            //    exception,
            //    It.IsAny<Func<object, Exception, string>>()), Times.Once());
        }

        [Fact]
        public async Task CreateProduct_DuplicateProduct_ReturnsBadRequest()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "Duplicate Product" };
            var expectedMessage = new { message = "Duplicate product not allowed for this product Duplicate Product" };
            _mockProductService.Setup(x => x.AddProductAsync(productDto)).ReturnsAsync(new BadRequestObjectResult(expectedMessage));
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.CreateProduct(productDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // Type check
            var badRequestObjectResult = (BadRequestObjectResult)result; // Cast result
            Assert.Equal(expectedMessage, badRequestObjectResult.Value); // Value check
            _mockProductService.Verify(x => x.AddProductAsync(productDto), Times.Once());
            //_mockLogger.Verify(x => x.Log(
            //    LogLevel.Warning,
            //    It.IsAny<EventId>(),
            //    It.Is<object>(o => o.ToString().Contains("Duplicate product not allowed")),
            //    null,
            //    It.IsAny<Func<object, Exception, string>>()), Times.Once());
        }

        [Fact]
        public async Task UpdateProduct_ValidProduct_ReturnsOkResult()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "Updated Product" };
            var expectedMessage = new { message = "Product Updated successfully", product = productDto };
            _mockProductService.Setup(x => x.UpdateProductAsync(productDto)).ReturnsAsync(new OkObjectResult(expectedMessage));
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.UpdateProduct(productDto);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            Assert.IsType<OkObjectResult>(result); // Type check
            var okObjectResult = (OkObjectResult)result; // Cast result
            Assert.Equal(expectedMessage, okObjectResult.Value); // Value check
            _mockProductService.Verify(x => x.UpdateProductAsync(productDto), Times.Once());
            _mockLogger.VerifyNoOtherCalls();
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task UpdateProduct_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "" };
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            // Act
            var result = await _controller.UpdateProduct(productDto);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            Assert.IsType<BadRequestObjectResult>(result); // Changed: Replaced FluentAssertions with xUnit assertion for type check
            var badRequestResult = (BadRequestObjectResult)result; // Changed: Replaced Subject with direct cast
            Assert.IsType<SerializableError>(badRequestResult.Value); // Changed: Replaced FluentAssertions with xUnit assertion for type check
            var serializableError = (SerializableError)badRequestResult.Value;
            Assert.IsType<string[]>(serializableError["Name"]); // Changed: Replaced FluentAssertions with xUnit assertion for type check
            var nameErrors = (string[])serializableError["Name"];
            Assert.Single(nameErrors); // Changed: Replaced FluentAssertions with xUnit assertion for single check
            Assert.Equal("The Name field is required.", nameErrors[0]); // Changed: Replaced FluentAssertions with xUnit assertion for value check
            _mockProductService.Verify(x => x.UpdateProductAsync(It.IsAny<ProductDTO>()), Times.Never());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task UpdateProduct_ProductNotFound_ReturnsBadRequest()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "Updated Product" };
            var expectedMessage = new { message = $"Product is not available for this {productDto.ProductId} productId" };
            _mockProductService.Setup(x => x.UpdateProductAsync(productDto)).ReturnsAsync(new BadRequestObjectResult(expectedMessage));
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.UpdateProduct(productDto);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            Assert.IsType<BadRequestObjectResult>(result); // Type check
            var badRequestObjectResult = (BadRequestObjectResult)result; // Cast result
            Assert.Equal(expectedMessage, badRequestObjectResult.Value); // Value check
            _mockProductService.Verify(x => x.UpdateProductAsync(productDto), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task UpdateProduct_ServiceThrowsException_Returns500()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "Updated Product" };
            _mockProductService.Setup(x => x.UpdateProductAsync(productDto)).ThrowsAsync(new Exception());
            _controller.ModelState.Clear();

            // Act
            var result = await _controller.UpdateProduct(productDto);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            Assert.IsType<ObjectResult>(result); // Type check
            var statusCodeResult = (ObjectResult)result; // Cast result
            Assert.Equal(500, statusCodeResult.StatusCode); // Status code check
            Assert.Equal("Internal server error", statusCodeResult.Value); // Value check
            _mockProductService.Verify(x => x.UpdateProductAsync(productDto), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsOkResult_WithProducts()
        {
            // Arrange
            var expectedProducts = new List<ProductDTO>
            {
        new ProductDTO { ProductId = 1, Name = "Test Product 1", Price = 10.99m },
        new ProductDTO { ProductId = 2, Name = "Test Product 2", Price = 20.99m }
            };

            _mockProductService.Setup(service => service.GetAllProductsAsync()).ReturnsAsync(expectedProducts);

            // Act
            var result = await _controller.GetAllProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDTO>>(okResult.Value);
            Assert.Equal(expectedProducts, returnedProducts); // Changed: Replaced FluentAssertions with xUnit assertion for value check
        }

        [Fact]
        public async Task GetProductById_ReturnsOkResult_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new ProductDTO { ProductId = productId, Name = "Test Product", Price = 10.99m };
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId)).ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); // Type check
            var returnedProduct = Assert.IsType<ProductDTO>(okResult.Value); // Type check
            Assert.Equal(expectedProduct.ProductId, returnedProduct.ProductId); // Check ProductId
            Assert.Equal(expectedProduct.Name, returnedProduct.Name); // Check Name
            Assert.Equal(expectedProduct.Price, returnedProduct.Price); // Check Price
        }


        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId)).ReturnsAsync((ProductDTO)null);

            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result); // Type check
        }


        [Fact]
        public async Task DeleteProduct_ValidId_ReturnsOkResult()
        {
            // Arrange
            int id = 1;
            var expectedMessage = new { message = "Product deleted successfully." };
            _mockProductService.Setup(x => x.DeleteProductAsync(id)).ReturnsAsync(new OkObjectResult(expectedMessage));

            // Act
            var result = await _controller.DeleteProduct(id);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            var okObjectResult = Assert.IsType<OkObjectResult>(result); // Type check
            Assert.Equal(expectedMessage, okObjectResult.Value); // Value check
            _mockProductService.Verify(x => x.DeleteProductAsync(id), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task DeleteProduct_ProductNotFound_ReturnsBadRequest()
        {
            // Arrange
            int id = 1;
            var expectedMessage = new { message = $"Product with ID {id} not found." };
            _mockProductService.Setup(x => x.DeleteProductAsync(id)).ReturnsAsync(new BadRequestObjectResult(expectedMessage));

            // Act
            var result = await _controller.DeleteProduct(id);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            var badRequestObjectResult = Assert.IsType<BadRequestObjectResult>(result); // Type check
            Assert.Equal(expectedMessage, badRequestObjectResult.Value); // Value check
            _mockProductService.Verify(x => x.DeleteProductAsync(id), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task DeleteProduct_ServiceThrowsException_Returns500()
        {
            // Arrange
            int id = 1;
            _mockProductService.Setup(x => x.DeleteProductAsync(id)).ThrowsAsync(new Exception());

            // Act
            var result = await _controller.DeleteProduct(id);

            // Assert
            Console.WriteLine("Assert Start: " + DateTime.Now);
            Assert.IsType<ObjectResult>(result); // Type check
            var statusCodeResult = (ObjectResult)result; // Cast result
            Assert.Equal(500, statusCodeResult.StatusCode); // Status code check
            Assert.Equal("Internal server error", statusCodeResult.Value); // Value check
            _mockProductService.Verify(x => x.DeleteProductAsync(id), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }
    }
}
