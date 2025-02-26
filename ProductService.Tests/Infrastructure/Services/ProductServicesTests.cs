using AutoMapper;
using FluentAssertions;
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
            var productDto = new ProductDTO { ProductId = 1, Name = "New Product" };
            var product = new Product { ProductId = 1, Name = "New Product" };

            _mockMapper.Setup(x => x.Map<Product>(productDto)).Returns(product);
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
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Product created successfully", product = productDto });
            productRepoMock.Verify(x => x.AddAsync(product), Times.Once());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
        }

        [Fact]
        public async Task GetAllProductsAsync_ReturnsCachedProducts_WhenCacheExists()
        {
            // Arrange
            var cachedProducts = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, Name = "Cached Product" }
            };

            _mockCacheService.Setup(c => c.GetAsync<IEnumerable<ProductDTO>>("products:all")).ReturnsAsync(cachedProducts);

            // Act
            var result = await _productServices.GetAllProductsAsync();

            // Assert
            result.Should().BeEquivalentTo(cachedProducts);
            _mockUnitOfWork.Verify(x => x.Repository<Product>().GetAllAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllProductsAsync_CacheMiss_ReturnsProducts()
        {
            // Arrange
            var productsFromRepo = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product1" },
                new Product { ProductId = 2, Name = "Product2" }
            };
            var productDtos = new List<ProductDTO>
            {
                new ProductDTO { ProductId = 1, Name = "Product1" },
                new ProductDTO { ProductId = 2, Name = "Product2" }
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
            result.Should().BeEquivalentTo(productDtos);
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
            result.Should().BeEquivalentTo(productDto);
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
            result.Should().BeNull();
        }

        [Fact]
        public async Task DeleteProductAsync_ProductExists_ReturnsOkResult()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId, Name = "Test Product" };

            var mockProductRepo = new Mock<IGenericRepository<Product>>();
            mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync(product);
            mockProductRepo.Setup(x => x.Remove(product)).Verifiable();
            _mockUnitOfWork.Setup(x => x.Repository<Product>()).Returns(mockProductRepo.Object);
            _mockUnitOfWork.Setup(x => x.CompleteAsync()).ReturnsAsync(1);

            _mockCacheService.Setup(x => x.RemoveAsync($"{ProductKeyPrefix}{productId}")).Returns(Task.FromResult(true));
            _mockCacheService.Setup(x => x.RemoveAsync("products:all")).Returns(Task.FromResult(true));

            _mockHttpContextAccessor.Setup(x => x.HttpContext.User.Identity.Name).Returns("testuser");
            _mockRabbitMQHelper.Setup(x => x.AuditResAsync(It.IsAny<AuditMessageDto>())).Returns(Task.FromResult(true))
               .Callback<AuditMessageDto>(dto =>
               {
                   Console.WriteLine($"AuditMessageDto: OprtnTyp={dto.OprtnTyp}, ScreenPk={dto.ScreenPk}, LogDsc={string.Join(", ", dto.LogDsc)}");
               });

            // Act
            Console.WriteLine("Assert Start: " + DateTime.Now);
            var result = await _productServices.DeleteProductAsync(productId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().BeEquivalentTo(new { message = "Product deleted successfully." });

            mockProductRepo.Verify(x => x.GetByIdAsync(productId), Times.Once());
            mockProductRepo.Verify(x => x.Remove(product), Times.Once());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Once());
            _mockCacheService.Verify(x => x.RemoveAsync($"{ProductKeyPrefix}{productId}"), Times.Once());
            _mockCacheService.Verify(x => x.RemoveAsync("products:all"), Times.Once());
            _mockRabbitMQHelper.Verify(x => x.AuditResAsync(It.Is<AuditMessageDto>
                (dto => dto.OprtnTyp == 3 && dto.ScreenPk != null && dto.LogDsc.Any(d => d.Contains("Deleted By testuser")))), Times.Once());
            Console.WriteLine("Test End: " + DateTime.Now);
        }

        [Fact]
        public async Task DeleteProductAsync_ProductNotFound_ReturnsBadRequest()
        {
            // Arrange
            var productId = 1;

            var mockProductRepo = new Mock<IGenericRepository<Product>>();
            mockProductRepo.Setup(x => x.GetByIdAsync(productId)).ReturnsAsync((Product)null);
            _mockUnitOfWork.Setup(x => x.Repository<Product>()).Returns(mockProductRepo.Object);

            // Act
            var result = await _productServices.DeleteProductAsync(productId);

            // Assert
            var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = $"Product with ID {productId} not found." });

            mockProductRepo.Verify(x => x.GetByIdAsync(productId), Times.Once());
            mockProductRepo.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never());
            _mockUnitOfWork.Verify(x => x.CompleteAsync(), Times.Never());
            _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never());
            _mockRabbitMQHelper.Verify(x => x.AuditResAsync(It.IsAny<AuditMessageDto>()), Times.Never());
        }
    }
}