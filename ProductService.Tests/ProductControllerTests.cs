using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.API.Controllers;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Services;

namespace ProductService.Tests
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
            returnedProducts.Should().BeEquivalentTo(expectedProducts);
        }

        [Fact]
        public async Task GetProductById_ReturnsOkResult_WhenProductExists()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new ProductDTO { ProductId = productId, Name = "Test Product", Price = 10.99m };

            _mockProductService
                .Setup(service => service.GetProductByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProduct = Assert.IsType<ProductDTO>(okResult.Value);
            returnedProduct.Should().BeEquivalentTo(expectedProduct);
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
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
