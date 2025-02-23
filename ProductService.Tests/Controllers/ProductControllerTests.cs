using FluentAssertions;
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
            var productDto = new ProductDTO { ProductId = 1, Name = "New Product" };
            var okResult = new OkObjectResult(new { message = "Product created successfully", product = productDto });

            _mockProductService.Setup(x => x.AddProductAsync(productDto)).ReturnsAsync(okResult);
            _controller.ModelState.Clear();

            var result = await _controller.CreateProduct(productDto);

            var okObjectResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(new { message = "Product created successfully", product = productDto });
            _mockProductService.Verify(x => x.AddProductAsync(productDto), Times.Once());
            _mockLogger.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never()); 
        }

        [Fact]
        public async Task CreateProduct_ServiceThrowsException_Returns500()
        {
            var productDto = new ProductDTO { ProductId = 1, Name = "New Product" };
            var exception = new Exception("Service error");

            _mockProductService.Setup(x => x.AddProductAsync(productDto)).ThrowsAsync(exception);
            _controller.ModelState.Clear();

            var result = await _controller.CreateProduct(productDto);

            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Internal server error");
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
            var productDto = new ProductDTO { ProductId = 1, Name = "Duplicate Product" };
            var badRequestResult = new BadRequestObjectResult(new { message = "Duplicate product not allowed for this product Duplicate Product" });

            _mockProductService.Setup(x => x.AddProductAsync(productDto)).ReturnsAsync(badRequestResult);
            _controller.ModelState.Clear();

            var result = await _controller.CreateProduct(productDto);

            var badRequestObjectResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestObjectResult.Value.Should().BeEquivalentTo(new { message = "Duplicate product not allowed for this product Duplicate Product" });
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
            var productDto = new ProductDTO { ProductId = 1, Name = "Updated Product" };
            var okResult = new OkObjectResult(new { message = "Product Updated successfully", product = productDto });

            _mockProductService.Setup(x => x.UpdateProductAsync(productDto)).ReturnsAsync(okResult);
            _controller.ModelState.Clear();

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.UpdateProduct(productDto);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var okObjectResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(new { message = "Product Updated successfully", product = productDto });
            _mockProductService.Verify(x => x.UpdateProductAsync(productDto), Times.Once());
            _mockLogger.VerifyNoOtherCalls();
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task UpdateProduct_InvalidModelState_ReturnsBadRequest()
        {
            var productDto = new ProductDTO { ProductId = 1, Name = "" }; 
            _controller.ModelState.AddModelError("Name", "The Name field is required.");

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.UpdateProduct(productDto);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            var serializableError = badRequestResult.Value.Should().BeOfType<SerializableError>().Subject;
            var nameErrors = serializableError["Name"].Should().BeOfType<string[]>().Subject;
            nameErrors.Should().ContainSingle().Which.Should().Be("The Name field is required.");
            _mockProductService.Verify(x => x.UpdateProductAsync(It.IsAny<ProductDTO>()), Times.Never());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task UpdateProduct_ProductNotFound_ReturnsBadRequest()
        {
            var productDto = new ProductDTO { ProductId = 1, Name = "Updated Product" };
            var badRequestResult = new BadRequestObjectResult(new { message = $"Product is not available for this {productDto.ProductId} productId" });

            _mockProductService.Setup(x => x.UpdateProductAsync(productDto)).ReturnsAsync(badRequestResult);
            _controller.ModelState.Clear();

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.UpdateProduct(productDto);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var badRequestObjectResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestObjectResult.Value.Should().BeEquivalentTo(new { message = $"Product is not available for this {productDto.ProductId} productId" });
            _mockProductService.Verify(x => x.UpdateProductAsync(productDto), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }


        [Fact]
        public async Task UpdateProduct_ServiceThrowsException_Returns500()
        {
            var productDto = new ProductDTO { ProductId = 1, Name = "Updated Product" };
            var exception = new Exception("Service error");

            _mockProductService.Setup(x => x.UpdateProductAsync(productDto)).ThrowsAsync(exception);
            _controller.ModelState.Clear();

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.UpdateProduct(productDto);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Internal server error");
            _mockProductService.Verify(x => x.UpdateProductAsync(productDto), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }


        [Fact]
        public async Task GetAllProducts_ReturnsOkResult_WithProducts()
        {
            var expectedProducts = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, Name = "Test Product 1", Price = 10.99m },
                new ProductDTO { ProductId = 2, Name = "Test Product 2", Price = 20.99m }
            };

            _mockProductService.Setup(service => service.GetAllProductsAsync()).ReturnsAsync(expectedProducts);

            var result = await _controller.GetAllProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDTO>>(okResult.Value);
            returnedProducts.Should().BeEquivalentTo(expectedProducts);
        }

        [Fact]
        public async Task GetProductById_ReturnsOkResult_WhenProductExists()
        {
            var productId = 1;
            var expectedProduct = new ProductDTO { ProductId = productId, Name = "Test Product", Price = 10.99m };

            _mockProductService
                .Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            var result = await _controller.GetProductById(productId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProduct = Assert.IsType<ProductDTO>(okResult.Value);
            returnedProduct.Should().BeEquivalentTo(expectedProduct);
        }

        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var productId = 999;
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId)).ReturnsAsync((ProductDTO)null);

            var result = await _controller.GetProductById(productId);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ValidId_ReturnsOkResult()
        {
            int id = 1;
            var okResult = new OkObjectResult(new { message = "Product deleted successfully." });

            _mockProductService.Setup(x => x.DeleteProductAsync(id)).ReturnsAsync(okResult);

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.DeleteProduct(id);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var okObjectResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okObjectResult.Value.Should().BeEquivalentTo(new { message = "Product deleted successfully." });
            _mockProductService.Verify(x => x.DeleteProductAsync(id), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task DeleteProduct_ProductNotFound_ReturnsBadRequest()
        {
            int id = 1;
            var badRequestResult = new BadRequestObjectResult(new { message = $"Product with ID {id} not found." });

            _mockProductService.Setup(x => x.DeleteProductAsync(id)).ReturnsAsync(badRequestResult);

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.DeleteProduct(id);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var badRequestObjectResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestObjectResult.Value.Should().BeEquivalentTo(new { message = $"Product with ID {id} not found." });
            _mockProductService.Verify(x => x.DeleteProductAsync(id), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task DeleteProduct_ServiceThrowsException_Returns500()
        {
            int id = 1;
            var exception = new Exception("Service error");

            _mockProductService.Setup(x => x.DeleteProductAsync(id)).ThrowsAsync(exception);

            Console.WriteLine("Arrange Start: " + DateTime.Now);

            Console.WriteLine("Act Start: " + DateTime.Now);
            var result = await _controller.DeleteProduct(id);

            Console.WriteLine("Assert Start: " + DateTime.Now);
            var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
            statusCodeResult.StatusCode.Should().Be(500);
            statusCodeResult.Value.Should().Be("Internal server error");
            _mockProductService.Verify(x => x.DeleteProductAsync(id), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }
    }
}
