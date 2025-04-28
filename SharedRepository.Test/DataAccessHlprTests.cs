using Microsoft.EntityFrameworkCore;
using ProductService.API.Infrastructure.DBContext;
using ProductService.API.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using SharedRepository.Repositories;
using Moq;
using Xunit;
using Dapper;

namespace SharedRepository.Test
{
    //public class DataAccessHlprTests
    //{
    //    private readonly Mock<IDbConnection> _mockDbConnection;
    //    private readonly Mock<IDbCommand> _mockDbCommand;
    //    private readonly Mock<IDbTransaction> _mockTransaction;
    //    private readonly Mock<IDataAccessHelper> _mockDataAccessHelper;

    //    public DataAccessHlprTests()
    //    {
    //        _mockDbConnection = new Mock<IDbConnection>();
    //        _mockDbCommand = new Mock<IDbCommand>();
    //        _mockTransaction = new Mock<IDbTransaction>();
    //        _mockDataAccessHelper = new Mock<IDataAccessHelper>();
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ReturnsTrue_WhenRecordExists()
    //    {
    //        Arrange
    //       var tableName = "Users";
    //        var idColumn = "Id";
    //        var idValue = 1;

    //        var mockDbConnection = new Mock<IDbConnection>();
    //        mockDbConnection
    //            .Setup(conn => conn.QuerySingleOrDefaultAsync<int>(
    //                It.IsAny<string>(), It.IsAny<object>(), null, null, null))
    //            .ReturnsAsync(1); // Simulating that a record exists

    //        var dataAccessHelper = new DataAccessHlpr();
    //        typeof(DataAccessHlpr)
    //            .GetProperty("DbConnection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
    //            ?.SetValue(dataAccessHelper, mockDbConnection.Object);

    //        Act
    //       var result = await dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        Assert
    //        Assert.True(result);
    //    }

        //public class DataAccessHlprTests
        //{
        //    private readonly Mock<IDbConnection> _mockDbConnection;
        //    private readonly DataAccessHlpr _dataAccessHelper;
        //    //private readonly Fixture _fixture;
        //    // Declare AutoFixture for generating dummy data.
        //    private readonly Fixture _fixture = new Fixture();
        //    public DataAccessHlprTests()
        //    {
        //        _mockDbConnection = new Mock<IDbConnection>();
        //        _dataAccessHelper = new DataAccessHlpr();
        //        //_fixture = new Fixture();
        //    }

        //    [Fact]
        //    public async Task ExistsAsync_ReturnsTrue_WhenRecordExists()
        //    {
        //        // Arrange
        //        string tableName = "Users";
        //        string idColumn = "Id";
        //        int idValue = 1;

        //        _mockDbConnection.SetupDapperAsync(c =>
        //                c.QuerySingleOrDefaultAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), null, null, null))
        //            .ReturnsAsync(1); // Simulate record exists

        //        // Act
        //        bool result = await _dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

        //        // Assert
        //        Assert.True(result);
        //    }

        //    [Fact]
        //    public async Task UpdateProductStockAsync_ReturnsTrue_WhenStockUpdated()
        //    {
        //        // Arrange
        //        int productId = 1;
        //        int quantity = 10;

        //        _mockDbConnection.SetupDapperAsync(c =>
        //                c.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), null, null))
        //            .ReturnsAsync(1); // Simulate successful update

        //        // Act
        //        bool result = await _dataAccessHelper.UpdateProductStockAsync(productId, quantity);

        //        // Assert
        //        Assert.True(result);
        //    }

        //    [Fact]
        //    public async Task UpdateProductStockByOrderedAsync_ReturnsFalse_WhenStockIsInsufficient()
        //    {
        //        // Arrange
        //        int productId = 1;
        //        int quantity = -15; // Trying to deduct more than available stock

        //        _mockDbConnection.SetupDapperAsync(c =>
        //                c.QuerySingleOrDefaultAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), null, null, null))
        //            .ReturnsAsync(5); // Simulate available stock is 5

        //        // Act
        //        bool result = await _dataAccessHelper.UpdateProductStockByOrderedAsync(productId, quantity);

        //        // Assert
        //        Assert.False(result);
        //    }

        //    [Fact]
        //    public async Task UpdateProductStockByOrderedAsync_ReturnsTrue_WhenStockIsUpdated()
        //    {
        //        // Arrange
        //        int productId = 1;
        //        int quantity = -5; // Deducting stock

        //        _mockDbConnection.SetupDapperAsync(c =>
        //                c.QuerySingleOrDefaultAsync<int>(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), null, null, null))
        //            .ReturnsAsync(10); // Simulate available stock is 10

        //        _mockDbConnection.SetupDapperAsync(c =>
        //                c.ExecuteAsync(It.IsAny<IDbConnection>(), It.IsAny<string>(), It.IsAny<object>(), null, null))
        //            .ReturnsAsync(1); // Simulate successful update

        //        // Act
        //        bool result = await _dataAccessHelper.UpdateProductStockByOrderedAsync(productId, quantity);

        //        // Assert
        //        Assert.True(result);
        //    }
    }



public class DataAccessHlprTests
{
    //[Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExist()
    {
        // Mock IDapperHelper
        var mockDapperHelper = new Mock<IDapperHelper>();
        mockDapperHelper
            .Setup(helper => helper.QuerySingleOrDefaultAsync<int>(
                It.IsAny<IDbConnection>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
            .ReturnsAsync(1); // Simulate record exists

        // Inject mock into DataAccessHlpr
        var dataAccessHelper = new DataAccessHlpr("FakeConnectionString", mockDapperHelper.Object);

        // Call ExistsAsync
        var result = await dataAccessHelper.ExistsAsync("Products", "ProductId", 1);

        // Assert result
        Assert.True(result);
    }

    //[Fact]
    //public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExists()
    //{
    //    // Step 1: Set up in-memory DbContext
    //    var options = new DbContextOptionsBuilder<ProductDbContext>()
    //        .UseInMemoryDatabase(databaseName: "TestDb")
    //        .Options;

    //    using (var context = new ProductDbContext(options))
    //    {
    //        // Step 2: Add test data
    //        context.Products.Add(new Product { ProductId = 1, Name = "Test Product", Stock = 10, Description = "Test Description", Price = 100, TaxPercentage = 10 });
    //        context.SaveChanges();

    //        // Step 3: Mock IDbConnection
    //        var mockConnection = new Mock<IDbConnection>();
    //        mockConnection
    //            .Setup(conn => conn.QuerySingleOrDefaultAsync<int>(
    //                It.IsAny<string>(),
    //                It.IsAny<object>(),
    //                null,
    //                null,
    //                null))
    //            .ReturnsAsync(1); // Simulating record exists

    //        // Step 4: Initialize DataAccessHelper
    //        var dataAccessHelper = new DataAccessHlpr();
    //        mockConnection.Setup(conn => conn.State).Returns(ConnectionState.Open);
    //        dataAccessHelper.DbConnection = mockConnection.Object;

    //        // Step 5: Execute ExistsAsync
    //        var result = await dataAccessHelper.ExistsAsync("Products", "ProductId", 1);

    //        // Step 6: Assert Result
    //        Assert.True(result);
    //    }
    //}
}

