using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.Services;
using ProductService.API.Infrastructure.UnitOfWork;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.RedisCache;
using SharedRepository.Repositories;
using System.Reflection;

namespace ProductService.Tests.Infrastructure.Services
{
    public class ProductServicesTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<ProductServices>> _mockLogger;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly Mock<IRabbitMQHelper> _mockRabbitMQHelper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        private readonly ProductServices _productServices;

        private const string ProductKeyPrefix = "product:";
        private const string ALL_PRODUCTS_KEY = "products:all";

        public ProductServicesTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<ProductServices>>();
            _mockCacheService = new Mock<ICacheService>();
            _mockRabbitMQHelper = new Mock<IRabbitMQHelper>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            
            _productServices = new ProductServices(
                _mockUnitOfWork.Object,
                _mockMapper.Object,
                _mockLogger.Object,
                _mockCacheService.Object,
                _mockRabbitMQHelper.Object,
                _mockHttpContextAccessor.Object
            );

        }

        [Fact]
        public async Task AddProductAsync_NewProduct_ReturnsOkResult()
        {
            // Arrange
            var productDto = new ProductDTO { ProductId = 1, Name = "New Product", Description = "New Description", Price = 60, Stock = 100, TaxPercentage = 10 };
            var product = new Product { ProductId = 1, Name = "New Product", Description = "New Description", Price = 60, Stock = 100, TaxPercentage = 10 };

            _mockMapper.Setup(x => x.Map<Product>(productDto)).Returns(product);
            _mockMapper.Setup(x => x.Map<ProductDTO>(product)).Returns(productDto); // Mock mapping back to DTO
            var productRepoMock = new Mock<IGenericRepository<Product>>();
            productRepoMock.Setup(x => x.AddAsync(product)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(x => x.Repository<Product>()).Returns(productRepoMock.Object);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);
            _mockCacheService.Setup(x => x.RemoveAsync("products:all")).Returns(Task.FromResult(true));
            _mockCacheService.Setup(x => x.SetAsync($"{ProductKeyPrefix}{product.ProductId}", productDto,
                           It.IsAny<TimeSpan>())).Returns(Task.FromResult(true));
            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("testuser");
            _mockRabbitMQHelper.Setup(x => x.AuditResAsync(It.IsAny<AuditMessageDto>())).Returns(Task.FromResult(true));

            // Act
            var result = await _productServices.AddProductAsync(productDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(200, okResult.StatusCode);

            object resultValue = okResult.Value;
            Assert.NotNull(resultValue);

            // Get the message
            PropertyInfo messageProperty = resultValue.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            string message = messageProperty.GetValue(resultValue) as string;
            Assert.Equal("Product created successfully", message);

            // Get the product
            PropertyInfo productProperty = resultValue.GetType().GetProperty("product");
            Assert.NotNull(productProperty);
            object actualProduct = productProperty.GetValue(resultValue);
            Assert.NotNull(actualProduct);

            // Assert product properties
            PropertyInfo productIdProperty = actualProduct.GetType().GetProperty("ProductId");
            Assert.NotNull(productIdProperty);
            Assert.Equal(productDto.ProductId, productIdProperty.GetValue(actualProduct));

            PropertyInfo nameProperty = actualProduct.GetType().GetProperty("Name");
            Assert.NotNull(nameProperty);
            Assert.Equal(productDto.Name, nameProperty.GetValue(actualProduct));

            productRepoMock.Verify(x => x.AddAsync(product), Times.Once());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsCachedProducts_WhenCacheExists()
        {
            // Arrange
            var cachedProducts = new List<ProductDTO>   
            { 
              new ProductDTO { ProductId = 1, Name = "Cached Product", Description = "Cached Description", Price = 70, Stock = 100, TaxPercentage = 10}
            };
            _mockCacheService.Setup(x => x.GetAsync<IEnumerable<ProductDTO>>("products:all")).ReturnsAsync(cachedProducts);

            // Act
            var result = await _productServices.GetAllProductsAsync();

            // Assert
            Assert.Equal(cachedProducts, result); 
            _mockUnitOfWork.Verify(x => x.Repository<Product>().GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllProductsAsync_CacheMiss_ReturnsProducts()
        {
            // Arrange
            var productsFromRepo = new List<Product>
            {
                new Product { ProductId = 1, Name = "ProductName", Description ="Product Description", Price = 70, Stock = 100, TaxPercentage = 10},
                new Product { ProductId = 2, Name = "ProductName", Description ="Product Description", Price = 70, Stock = 100, TaxPercentage = 10 }
            };
            var productDtos = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, Name = "ProductName", Description ="Product Description", Price = 70, Stock = 100, TaxPercentage = 10 },
                new ProductDTO { ProductId = 2, Name = "ProductName", Description ="Product Description", Price = 70, Stock = 100, TaxPercentage = 10 }
            };

            _mockCacheService.Setup(x => x.GetAsync<IEnumerable<ProductDTO>>(It.IsAny<string>()))
                             .ReturnsAsync((IEnumerable<ProductDTO>)null);

            var productRepoMock = new Mock<IGenericRepository<Product>>();
            productRepoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(productsFromRepo);
            _mockUnitOfWork.Setup(x => x.Repository<Product>()).Returns(productRepoMock.Object);
            _mockMapper.Setup(x => x.Map<IEnumerable<ProductDTO>>(productsFromRepo)).Returns(productDtos);
            _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<IEnumerable<ProductDTO>>(),
                           It.IsAny<TimeSpan>())).Returns(Task.FromResult(true));

            // Act
            var result = await _productServices.GetAllProductsAsync();

            // Assert
            Assert.Equal(productDtos, result);
            _mockCacheService.Verify(x => x.GetAsync<IEnumerable<ProductDTO>>("products:all"), Times.Once());
            _mockUnitOfWork.Verify(x => x.Repository<Product>(), Times.Once());
            productRepoMock.Verify(x => x.GetAllAsync(), Times.Once());
            _mockMapper.Verify(x => x.Map<IEnumerable<ProductDTO>>(productsFromRepo), Times.Exactly(2));
            _mockCacheService.Verify(x => x.SetAsync("products:all", It.Is<IEnumerable<ProductDTO>>
                (dtos => dtos.SequenceEqual(productDtos)), TimeSpan.FromMinutes(5)), Times.Once());
            _mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                     It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Never());
        }

        [Fact]
        public async Task GetProductByIdAsync_CacheMiss_ReturnsProductsFromDatabase()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId, Name = "Test Product" };
            var productDto = new ProductDTO { ProductId = productId, Name = "Test Product" };

            _mockCacheService.Setup(x => x.GetAsync<ProductDTO>(It.IsAny<string>())).ReturnsAsync((ProductDTO)null);
            _mockUnitOfWork.Setup(x => x.Repository<Product>().GetByIdAsync(productId)).ReturnsAsync(product);
            _mockMapper.Setup(x => x.Map<ProductDTO>(product)).Returns(productDto);
            _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<ProductDTO>(), It.IsAny<TimeSpan>()))
                             .Returns(Task.FromResult(true));

            // Act
            var result = await _productServices.GetProductByIdAsync(productId);

            // Assert
            Assert.Equal(productDto, result);
            _mockCacheService.Verify(x => x.SetAsync($"{ProductKeyPrefix}{productId}", productDto, It.IsAny<TimeSpan>()), Times.Once());
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductNotFound_ReturnsNull()
        {
            // Arrange
            var productId = 1;
            _mockCacheService.Setup(x => x.GetAsync<ProductDTO>(It.IsAny<string>())).ReturnsAsync((ProductDTO)null);
            _mockUnitOfWork.Setup(x => x.Repository<Product>().GetByIdAsync(productId)).ReturnsAsync((Product)null);

            // Act
            var result = await _productServices.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductExists_ReturnsOkResult()
        {
            // Arrange
            int productId = 1;
            var product = new Product { ProductId = productId, Name = "Test Product" };

            _mockUnitOfWork.Setup(x => x.Repository<Product>().GetByIdAsync(productId)).ReturnsAsync(product);

            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("testuser");

            // Act
            var result = await _productServices.DeleteProductAsync(productId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.Equal(200, okResult.StatusCode);

            // Use reflection to get the 'message' property
            object resultValue = okResult.Value;
            Assert.NotNull(resultValue);

            PropertyInfo messageProperty = resultValue.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            string message = messageProperty.GetValue(resultValue) as string;
            Assert.Equal("Product deleted successfully.", message);

            _mockUnitOfWork.Verify(x => x.Repository<Product>().Remove(product), Times.Once);
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once);
            _mockCacheService.Verify(cache => cache.RemoveAsync($"product:{productId}"), Times.Once);
            _mockCacheService.Verify(cache => cache.RemoveAsync("products:all"), Times.Once);
            _mockRabbitMQHelper.Verify(rmq => rmq.AuditResAsync(It.IsAny<AuditMessageDto>()), Times.Once);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductDoesNotExist_ReturnsBadRequestResult()
        {
            // Arrange
            int productId = 1;
            _mockUnitOfWork.Setup(x => x.Repository<Product>().GetByIdAsync(productId)).ReturnsAsync((Product)null); 

            // Act
            var result = await _productServices.DeleteProductAsync(productId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.Equal(400, badRequestResult.StatusCode);

            // Use reflection to get the 'message' property
            object resultValue = badRequestResult.Value;
            Assert.NotNull(resultValue);

            PropertyInfo messageProperty = resultValue.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);

            string message = messageProperty.GetValue(resultValue) as string;
            Assert.Equal($"Product with ID {productId} not found.", message);

            _mockUnitOfWork.Verify(uow => uow.Repository<Product>().Remove(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(uow => uow.CompleteAsync(), Times.Never);
            _mockCacheService.Verify(cache => cache.RemoveAsync(It.IsAny<string>()), Times.Never);
            _mockRabbitMQHelper.Verify(rmq => rmq.AuditResAsync(It.IsAny<AuditMessageDto>()), Times.Never);
        }
    }
}