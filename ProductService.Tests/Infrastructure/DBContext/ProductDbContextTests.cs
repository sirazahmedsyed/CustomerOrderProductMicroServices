using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.API.Infrastructure.DBContext;
using ProductService.API.Infrastructure.Entities;
using Xunit;

namespace ProductService.Tests.Infrastructure.DBContext
{
    public class ProductDbContextTests
    {
        private ProductDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid().ToString()}")
                .Options;
            return new ProductDbContext(options);
        }

        [Fact]
        public void ProductDbContext_ConfiguresProductTableName()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var tableName = entityType.GetTableName();

            tableName.Should().Be("products");
        }

        [Fact]
        public void ProductDbContext_ConfiguresProductIdColumn()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var property = entityType.FindProperty(nameof(Product.ProductId));
            var columnName = property.GetColumnName();

            columnName.Should().Be("product_id");
        }

        [Fact]
        public void ProductDbContext_ConfiguresNameColumn()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var property = entityType.FindProperty(nameof(Product.Name));
            var columnName = property.GetColumnName();

            columnName.Should().Be("name");
        }

        [Fact]
        public void ProductDbContext_ConfiguresDescriptionColumn()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var property = entityType.FindProperty(nameof(Product.Description));
            var columnName = property.GetColumnName();

            columnName.Should().Be("description");
        }

        [Fact]
        public void ProductDbContext_ConfiguresPriceColumn()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var property = entityType.FindProperty(nameof(Product.Price));
            var columnName = property.GetColumnName();

            columnName.Should().Be("price");
        }

        [Fact]
        public void ProductDbContext_ConfiguresStockColumn()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var property = entityType.FindProperty(nameof(Product.Stock));
            var columnName = property.GetColumnName();

            columnName.Should().Be("stock");
        }

        [Fact]
        public void ProductDbContext_ConfiguresTaxPercentageColumn()
        {
            using var dbContext = CreateInMemoryDbContext();

            var entityType = dbContext.Model.FindEntityType(typeof(Product));
            var property = entityType.FindProperty(nameof(Product.TaxPercentage));
            var columnName = property.GetColumnName();

            columnName.Should().Be("tax_percentage");
        }
    }
}
