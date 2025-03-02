using Microsoft.EntityFrameworkCore;
using Moq;
using ProductService.API.Infrastructure.DBContext;
using ProductService.API.Infrastructure.Entities;

namespace ProductService.Tests.Infrastructure.UnitOfWork
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly Mock<ProductDbContext> _mockContext;
        private readonly API.Infrastructure.UnitOfWork.UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _mockContext = new Mock<ProductDbContext>(options);
            _unitOfWork = new API.Infrastructure.UnitOfWork.UnitOfWork(_mockContext.Object);
        }

        [Fact]
        public void Repository_CalledMultipleTimesWithSameType_ReturnsSameInstance()
        {
            // Act
            var repository1 = _unitOfWork.Repository<Product>();
            var repository2 = _unitOfWork.Repository<Product>();

            // Assert
            Assert.Same(repository1, repository2); 
        }

        [Fact]
        public void Repository_CalledWithDifferentTypes_ReturnsDifferentInstances()
        {
            // Act
            var productRepository = _unitOfWork.Repository<Product>();
            var otherRepository = _unitOfWork.Repository<Category>();

            // Assert
            Assert.NotSame(productRepository, otherRepository); 
        }

        [Fact]
        public async Task CompleteAsync_SaveChangesAsyncIsCalled_ReturnsExpectedResult()
        {
            // Arrange
            _mockContext.Setup(c => c.SaveChangesAsync(default)).ReturnsAsync(1);

            // Act
            var result = await _unitOfWork.CompleteAsync();

            // Assert
            Assert.Equal(1, result); 
            _mockContext.Verify(c => c.SaveChangesAsync(default), Times.Once); 
        }

        [Fact]
        public void Dispose_ContextIsDisposed()
        {
            // Act
            _unitOfWork.Dispose();

            // Assert
            _mockContext.Verify(c => c.Dispose(), Times.Once); 
        }

        public void Dispose()
        {
            _unitOfWork.Dispose(); 
        }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
