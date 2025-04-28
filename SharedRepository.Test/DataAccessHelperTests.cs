using Moq;
using Npgsql;
using SharedRepository.Repositories;
using System.Data;
using Xunit;
using GrpcClient;
using System;
using System.Threading.Tasks;
using Dapper;
using GrpcService;
using Moq.Dapper;
using SharedRepository.Test;
using System.Reflection;
using Microsoft.Data.Sqlite;
using SQLitePCL;

namespace SharedRepository.Tests
{

    public class DataAccessHelperTests : IDisposable
    {
        // In-memory SQLite connection
        private readonly SqliteConnection _sqliteConnection;

        // Mock dependencies
        private readonly Mock<InactiveFlagClient> _mockInactiveFlagClient;
        private readonly Mock<ProductDetailsClient> _mockProductDetailsClient;
        private readonly Mock<CustomerClient> _mockCustomerClient;
        private readonly Mock<IDataAccessHelper> _mockDataAccessHelper;


        // SUT (System Under Test)

        public DataAccessHelperTests()
        {
            // Initialize SQLitePCL.raw provider.  Crucial!
            Batteries.Init();

            // Setup in-memory SQLite
            _sqliteConnection = new SqliteConnection("Data Source=:memory:");
            _sqliteConnection.Open();

            // Create test table
            using (var command = _sqliteConnection.CreateCommand())
            {
                command.CommandText = @" CREATE TABLE products (product_id INTEGER PRIMARY KEY, name TEXT);
                                     INSERT INTO products (product_id, name) VALUES (1, 'Test Product'); ";
                command.ExecuteNonQuery();
            }

            // Initialize mocks
            _mockInactiveFlagClient = new Mock<InactiveFlagClient>();
            _mockProductDetailsClient = new Mock<ProductDetailsClient>();
            _mockCustomerClient = new Mock<CustomerClient>();
            _mockDataAccessHelper = new Mock<IDataAccessHelper>();


        }

        public void Dispose()
        {
            _sqliteConnection?.Close();
            _sqliteConnection?.Dispose();
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExists()
        {
            // Arrange
            string tableName = "products";
            string idColumn = "product_id";
            int idValue = 1; // This exists in our test DB

            _mockDataAccessHelper.Setup(x => x.ExistsAsync(tableName, idColumn, idValue)).ReturnsAsync(true);
            // Act
            var result = await _mockDataAccessHelper.Object.ExistsAsync(tableName, idColumn, idValue);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_WhenRecordDoesNotExist()
        {
            // Arrange
            string tableName = "products";
            string idColumn = "product_id";
            int idValue = 999; // This doesn't exist in our test DB
            _mockDataAccessHelper.Setup(x => x.ExistsAsync(tableName, idColumn, idValue)).ReturnsAsync(false);


            // Act
            var result = await _mockDataAccessHelper.Object.ExistsAsync(tableName, idColumn, idValue);

            // Assert
            Assert.False(result);
        }
    }

    //public class DataAccessHelperTests : IDisposable
    //{
    //    // In-memory SQLite connection
    //    private readonly SqliteConnection _sqliteConnection;

    //    // Mock dependencies
    //    private readonly Mock<InactiveFlagClient> _mockInactiveFlagClient;
    //    private readonly Mock<ProductDetailsClient> _mockProductDetailsClient;
    //    private readonly Mock<CustomerClient> _mockCustomerClient;
    //    private readonly Mock<IDataAccessHelper> _mockDataAccessHelper;

    //    // SUT (System Under Test)
    //    private readonly DataAccessHelper _dataAccessHelper;

    //    public DataAccessHelperTests()
    //    {
    //        // Initialize SQLitePCL.raw provider.  Crucial!
    //        Batteries.Init();
    //        // Setup in-memory SQLite
    //        _sqliteConnection = new SqliteConnection("Data Source=:memory:");
    //        _sqliteConnection.Open();

    //        // Create test table
    //        using (var command = _sqliteConnection.CreateCommand())
    //        {
    //            command.CommandText = @" CREATE TABLE products (product_id INTEGER PRIMARY KEY, name TEXT);
    //                                     INSERT INTO products (product_id, name) VALUES (1, 'Test Product'); ";
    //            command.ExecuteNonQuery();
    //        }

    //        // Initialize mocks
    //        _mockInactiveFlagClient = new Mock<InactiveFlagClient>();
    //        _mockProductDetailsClient = new Mock<ProductDetailsClient>();
    //        _mockCustomerClient = new Mock<CustomerClient>();
    //        _mockDataAccessHelper = new Mock<IDataAccessHelper>();

    //        // Create instance of the class to be tested with mocked dependencies
    //        _dataAccessHelper = new DataAccessHelper(
    //            _mockInactiveFlagClient.Object,
    //            _mockProductDetailsClient.Object,
    //            _mockCustomerClient.Object
    //        );

    //        // Use reflection to replace the private dbconnection field
    //        // First, get the field info
    //        var dbConnectionProperty = typeof(DataAccessHelper)
    //            .GetProperty("DbConnection", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

    //        if (dbConnectionProperty != null)
    //        {
    //            // If it's a property
    //            dbConnectionProperty.SetValue(_dataAccessHelper, _sqliteConnection);
    //        }
    //        else
    //        {
    //            // Try as a field if it's not a property
    //            var dbConnectionField = typeof(DataAccessHelper)
    //                .GetField("DbConnection", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

    //            if (dbConnectionField != null)
    //            {
    //                dbConnectionField.SetValue(_dataAccessHelper, _sqliteConnection);
    //            }
    //        }

    //        // Also need to replace the private dbconnection string field with a dummy string
    //        // to prevent the real connection from being created
    //        var dbConnectionStringField = typeof(DataAccessHelper)
    //            .GetField("dbconnection", BindingFlags.NonPublic | BindingFlags.Instance);

    //        if (dbConnectionStringField != null)
    //        {
    //            dbConnectionStringField.SetValue(_dataAccessHelper, "Data Source=:memory:");
    //        }
    //    }

    //    public void Dispose()
    //    {
    //        _sqliteConnection?.Close();
    //        _sqliteConnection?.Dispose();
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExists()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 1; // This exists in our test DB

    //        _mockDataAccessHelper.Setup(x => x.ExistsAsync(tableName, idColumn, idValue)).ReturnsAsync(true);

    //        // Act
    //        var result = await _dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.True(result);
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnFalse_WhenRecordDoesNotExist()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 999; // This doesn't exist in our test DB

    //        // Act
    //        var result = await _dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.False(result);
    //    }
//}


    //public class DataAccessHelperTests
    //{
    //    // Mock dependencies
    //    private readonly Mock<InactiveFlagClient> _mockInactiveFlagClient;
    //    private readonly Mock<ProductDetailsClient> _mockProductDetailsClient;
    //    private readonly Mock<CustomerClient> _mockCustomerClient;

    //    public DataAccessHelperTests()
    //    {
    //        // Initialize mocks
    //        _mockInactiveFlagClient = new Mock<InactiveFlagClient>();
    //        _mockProductDetailsClient = new Mock<ProductDetailsClient>();
    //        _mockCustomerClient = new Mock<CustomerClient>();
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExists()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 1;

    //        // Create testable helper that returns true
    //        var dataAccessHelper = new TestableDataAccessHelper(
    //            true, // Exists should return true
    //            _mockInactiveFlagClient.Object,
    //            _mockProductDetailsClient.Object,
    //            _mockCustomerClient.Object
    //        );

    //        // Act
    //        var result = await dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.True(result);
    //        Assert.Equal((tableName, idColumn, idValue), dataAccessHelper.CalledWithParameters);
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnFalse_WhenRecordDoesNotExist()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 999; // Non-existent ID

    //        // Create testable helper that returns false
    //        var dataAccessHelper = new TestableDataAccessHelper(
    //            false, // Exists should return false
    //            _mockInactiveFlagClient.Object,
    //            _mockProductDetailsClient.Object,
    //            _mockCustomerClient.Object
    //        );

    //        // Act
    //        var result = await dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.False(result);
    //        Assert.Equal((tableName, idColumn, idValue), dataAccessHelper.CalledWithParameters);
    //    }
    //}


    //public class DataAccessHelperTests
    //{
    //    // Mock dependencies
    //    private readonly Mock<InactiveFlagClient> _mockInactiveFlagClient;
    //    private readonly Mock<ProductDetailsClient> _mockProductDetailsClient;
    //    private readonly Mock<CustomerClient> _mockCustomerClient;
    //    private readonly Mock<IDbConnection> _mockDbConnection;

    //    // SUT (System Under Test)
    //    private readonly TestableDataAccessHelper _dataAccessHelper;

    //    public DataAccessHelperTests()
    //    {
    //        // Initialize mocks
    //        _mockInactiveFlagClient = new Mock<InactiveFlagClient>();
    //        _mockProductDetailsClient = new Mock<ProductDetailsClient>();
    //        _mockCustomerClient = new Mock<CustomerClient>();
    //        _mockDbConnection = new Mock<IDbConnection>();

    //        // Create instance of the wrapper class with mocked dependencies
    //        _dataAccessHelper = new TestableDataAccessHelper(
    //            _mockDbConnection.Object,
    //            _mockInactiveFlagClient.Object,
    //            _mockProductDetailsClient.Object,
    //            _mockCustomerClient.Object
    //        );
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExists()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 1;
    //        string expectedQuery = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";

    //        // Setup DbConnection mock to return 1 (indicating record exists)
    //        _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(
    //            It.Is<string>(sql => sql == expectedQuery),
    //            It.Is<object>(param => param.GetType().GetProperty("IdValue").GetValue(param).Equals(idValue)),
    //            null, null, null))
    //            .ReturnsAsync(1);

    //        // Act
    //        var result = await _dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.True(result);
    //        _mockDbConnection.Verify(c => c.QuerySingleOrDefaultAsync<int>(
    //            It.Is<string>(sql => sql == expectedQuery),
    //            It.Is<object>(param => param.GetType().GetProperty("IdValue").GetValue(param).Equals(idValue)),
    //            null, null, null), Times.Once);
    //    }
    //}

    //public class DataAccessHelperTests
    //{
    //    // Mock dependencies
    //    private readonly Mock<InactiveFlagClient> _mockInactiveFlagClient;
    //    private readonly Mock<ProductDetailsClient> _mockProductDetailsClient;
    //    private readonly Mock<CustomerClient> _mockCustomerClient;
    //    private readonly Mock<IDbConnection> _mockDbConnection;

    //    // SUT (System Under Test)
    //    private readonly DataAccessHelper _dataAccessHelper;

    //    public DataAccessHelperTests()
    //    {
    //        // Initialize mocks
    //        _mockInactiveFlagClient = new Mock<InactiveFlagClient>();
    //        _mockProductDetailsClient = new Mock<ProductDetailsClient>();
    //        _mockCustomerClient = new Mock<CustomerClient>();
    //        _mockDbConnection = new Mock<IDbConnection>();

    //        // Create instance of the class to be tested with mocked dependencies
    //        _dataAccessHelper = new DataAccessHelper(
    //            _mockInactiveFlagClient.Object,
    //            _mockProductDetailsClient.Object,
    //            _mockCustomerClient.Object
    //        );

    //        // Use reflection to set the private DbConnection field to our mock
    //        var dbConnectionProperty = typeof(DataAccessHelper).GetProperty("DbConnection",
    //            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
    //        dbConnectionProperty.SetValue(_dataAccessHelper, _mockDbConnection.Object);
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnTrue_WhenRecordExists()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 1;
    //        string expectedQuery = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";

    //        // Setup DbConnection mock to return 1 (indicating record exists)
    //        _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(
    //            It.Is<string>(sql => sql == expectedQuery),
    //            It.Is<object>(param => param.GetType().GetProperty("IdValue").GetValue(param).Equals(idValue)),
    //            null, null, null))
    //            .ReturnsAsync(1);

    //        // Act
    //        var result = await _dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.True(result);
    //        //_mockDbConnection.Verify(c => c.QuerySingleOrDefaultAsync<int>(
    //        //    It.Is<string>(sql => sql == expectedQuery),
    //        //    It.Is<object>(param => param.GetType().GetProperty("IdValue").GetValue(param).Equals(idValue)),
    //        //    null, null, null), Times.Once);
    //    }

    //    [Fact]
    //    public async Task ExistsAsync_ShouldReturnFalse_WhenRecordDoesNotExist()
    //    {
    //        // Arrange
    //        string tableName = "products";
    //        string idColumn = "product_id";
    //        int idValue = 999; // Non-existent ID
    //        string expectedQuery = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";

    //        // Setup DbConnection mock to return 0 (indicating record doesn't exist)
    //        _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(
    //            It.Is<string>(sql => sql == expectedQuery),
    //            It.Is<object>(param => param.GetType().GetProperty("IdValue").GetValue(param).Equals(idValue)),
    //            null, null, null))
    //            .ReturnsAsync(0);

    //        // Act
    //        var result = await _dataAccessHelper.ExistsAsync(tableName, idColumn, idValue);

    //        // Assert
    //        Assert.False(result);
    //    }

    //    //[Fact]
    //    //public async Task GetProductDetailsAsync_ShouldReturnProductDetails_WhenProductExists()
    //    //{
    //    //    // Arrange
    //    //    int productId = 1;
    //    //    var expectedResponse = new ProductDetailsResponse
    //    //    {
    //    //        ProductId = productId,
    //    //        Name = "Test Product",
    //    //        Price = 99.99m
    //    //    };

    //    //    _mockProductDetailsClient
    //    //        .Setup(client => client.GetProductDetailsAsync(productId))
    //    //        .ReturnsAsync(expectedResponse);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.GetProductDetailsAsync(productId);

    //    //    // Assert
    //    //    Assert.NotNull(result);
    //    //    Assert.Equal(expectedResponse.ProductId, result.ProductId);
    //    //    Assert.Equal(expectedResponse.Name, result.Name);
    //    //    Assert.Equal(expectedResponse.Price, result.Price);
    //    //    _mockProductDetailsClient.Verify(client => client.GetProductDetailsAsync(productId), Times.Once);
    //    //}

    //    //[Fact]
    //    //public async Task GetProductDetailsAsync_ShouldThrowException_WhenGrpcClientFails()
    //    //{
    //    //    // Arrange
    //    //    int productId = 1;
    //    //    var expectedException = new Exception("GRPC client error");

    //    //    _mockProductDetailsClient
    //    //        .Setup(client => client.GetProductDetailsAsync(productId))
    //    //        .ThrowsAsync(expectedException);

    //    //    // Act & Assert
    //    //    var exception = await Assert.ThrowsAsync<Exception>(() =>
    //    //        _dataAccessHelper.GetProductDetailsAsync(productId));

    //    //    Assert.Equal(expectedException.Message, exception.Message);
    //    //}

    //    //[Fact]
    //    //public async Task GetInactiveFlagFromGrpcAsync_ShouldReturnInactiveFlag()
    //    //{
    //    //    // Arrange
    //    //    int userGroupNo = 42;
    //    //    bool expectedFlag = true;

    //    //    _mockInactiveFlagClient
    //    //        .Setup(client => client.GetInactiveFlagAsync(userGroupNo))
    //    //        .ReturnsAsync(expectedFlag);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.GetInactiveFlagFromGrpcAsync(userGroupNo);

    //    //    // Assert
    //    //    Assert.Equal(expectedFlag, result);
    //    //    _mockInactiveFlagClient.Verify(client => client.GetInactiveFlagAsync(userGroupNo), Times.Once);
    //    //}

    //    //[Fact]
    //    //public async Task GetInactiveCustomerFlag_ShouldReturnCustomerInactiveFlag()
    //    //{
    //    //    // Arrange
    //    //    Guid customerId = Guid.NewGuid();
    //    //    bool expectedFlag = true;

    //    //    _mockInactiveFlagClient
    //    //        .Setup(client => client.GetInactiveCustomerFlagAsync(customerId))
    //    //        .ReturnsAsync(expectedFlag);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.GetInactiveCustomerFlag(customerId);

    //    //    // Assert
    //    //    Assert.Equal(expectedFlag, result);
    //    //    _mockInactiveFlagClient.Verify(client => client.GetInactiveCustomerFlagAsync(customerId), Times.Once);
    //    //}

    //    //[Fact]
    //    //public async Task CheckEmailExistsAsync_ShouldReturnEmailResponse()
    //    //{
    //    //    // Arrange
    //    //    string email = "test@example.com";
    //    //    var expectedResponse = new EmailResponse { Exists = true, Email = email };

    //    //    _mockCustomerClient
    //    //        .Setup(client => client.CheckEmailExistsAsync(email))
    //    //        .ReturnsAsync(expectedResponse);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.CheckEmailExistsAsync(email);

    //    //    // Assert
    //    //    Assert.NotNull(result);
    //    //    Assert.Equal(expectedResponse.Exists, result.Exists);
    //    //    Assert.Equal(expectedResponse.Email, result.Email);
    //    //    _mockCustomerClient.Verify(client => client.CheckEmailExistsAsync(email), Times.Once);
    //    //}

    //    //[Fact]
    //    //public async Task UpdateProductStockAsync_ShouldReturnTrue_WhenUpdateSucceeds()
    //    //{
    //    //    // Arrange
    //    //    int productId = 1;
    //    //    int quantity = 10;
    //    //    string expectedQuery = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";

    //    //    // Setup the mock to return 1 affected row
    //    //    _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(
    //    //        It.Is<string>(sql => sql == expectedQuery),
    //    //        It.Is<object>(param =>
    //    //            param.GetType().GetProperty("Quantity").GetValue(param).Equals(quantity) &&
    //    //            param.GetType().GetProperty("ProductId").GetValue(param).Equals(productId)),
    //    //        null, null, null))
    //    //        .ReturnsAsync(1);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.UpdateProductStockAsync(productId, quantity);

    //    //    // Assert
    //    //    Assert.True(result);
    //    //    _mockDbConnection.Verify(c => c.ExecuteAsync(
    //    //        It.Is<string>(sql => sql == expectedQuery),
    //    //        It.Is<object>(param =>
    //    //            param.GetType().GetProperty("Quantity").GetValue(param).Equals(quantity) &&
    //    //            param.GetType().GetProperty("ProductId").GetValue(param).Equals(productId)),
    //    //        null, null, null), Times.Once);
    //    //}

    //    //[Fact]
    //    //public async Task UpdateProductStockAsync_ShouldReturnFalse_WhenNoRowsAffected()
    //    //{
    //    //    // Arrange
    //    //    int productId = 999; // Non-existent product
    //    //    int quantity = 5;
    //    //    string expectedQuery = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";

    //    //    // Setup the mock to return 0 affected rows
    //    //    _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(
    //    //        It.Is<string>(sql => sql == expectedQuery),
    //    //        It.Is<object>(param =>
    //    //            param.GetType().GetProperty("Quantity").GetValue(param).Equals(quantity) &&
    //    //            param.GetType().GetProperty("ProductId").GetValue(param).Equals(productId)),
    //    //        null, null, null))
    //    //        .ReturnsAsync(0);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.UpdateProductStockAsync(productId, quantity);

    //    //    // Assert
    //    //    Assert.False(result);
    //    //}

    //    //[Fact]
    //    //public async Task UpdateProductStockByOrderedAsync_ShouldReturnTrue_WhenStockIsAvailable()
    //    //{
    //    //    // Arrange
    //    //    int productId = 1;
    //    //    int quantity = -5; // Reducing stock
    //    //    int currentStock = 10; // More than enough stock

    //    //    string stockQuery = "SELECT stock FROM products WHERE product_id = @ProductId";
    //    //    string updateQuery = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";

    //    //    // Setup the mock to return current stock
    //    //    _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(
    //    //        It.Is<string>(sql => sql == stockQuery),
    //    //        It.Is<object>(param => param.GetType().GetProperty("ProductId").GetValue(param).Equals(productId)),
    //    //        null, null, null))
    //    //        .ReturnsAsync(currentStock);

    //    //    // Setup the mock to successfully update the stock
    //    //    _mockDbConnection.SetupDapperAsync(c => c.ExecuteAsync(
    //    //        It.Is<string>(sql => sql == updateQuery),
    //    //        It.Is<object>(param =>
    //    //            param.GetType().GetProperty("Quantity").GetValue(param).Equals(quantity) &&
    //    //            param.GetType().GetProperty("ProductId").GetValue(param).Equals(productId)),
    //    //        null, null, null))
    //    //        .ReturnsAsync(1);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.UpdateProductStockByOrderedAsync(productId, quantity);

    //    //    // Assert
    //    //    Assert.True(result);
    //    //}

    //    //[Fact]
    //    //public async Task UpdateProductStockByOrderedAsync_ShouldReturnFalse_WhenInsufficientStock()
    //    //{
    //    //    // Arrange
    //    //    int productId = 1;
    //    //    int quantity = -10; // Trying to reduce stock by 10
    //    //    int currentStock = 5;  // Only 5 in stock

    //    //    string stockQuery = "SELECT stock FROM products WHERE product_id = @ProductId";

    //    //    // Setup the mock to return current stock that's less than the requested reduction
    //    //    _mockDbConnection.SetupDapperAsync(c => c.QuerySingleOrDefaultAsync<int>(
    //    //        It.Is<string>(sql => sql == stockQuery),
    //    //        It.Is<object>(param => param.GetType().GetProperty("ProductId").GetValue(param).Equals(productId)),
    //    //        null, null, null))
    //    //        .ReturnsAsync(currentStock);

    //    //    // Act
    //    //    var result = await _dataAccessHelper.UpdateProductStockByOrderedAsync(productId, quantity);

    //    //    // Assert
    //    //    Assert.False(result);
    //    //    // Verify that no update query was executed
    //    //    _mockDbConnection.Verify(c => c.ExecuteAsync(
    //    //        It.IsAny<string>(),
    //    //        It.IsAny<object>(),
    //    //        null, null, null), Times.Never);
    //    //}
    //}

    // public class DataAccessHelperTests
    // {
    //     private readonly Mock<InactiveFlagClient> _inactiveFlagClientMock;
    //     private readonly Mock<ProductDetailsClient> _productDetailsClientMock;
    //     private readonly Mock<CustomerClient> _customerClientMock;
    //     private readonly DataAccessHelper _dataAccessHelper;

    //     public DataAccessHelperTests()
    //     {
    //         _inactiveFlagClientMock = new Mock<InactiveFlagClient>();
    //         _productDetailsClientMock = new Mock<ProductDetailsClient>();
    //         _customerClientMock = new Mock<CustomerClient>();

    //         _dataAccessHelper = new DataAccessHelper(
    //             _inactiveFlagClientMock.Object,
    //             _productDetailsClientMock.Object,
    //             _customerClientMock.Object);
    //     }

    //     [Fact]
    //     public async Task ExistsAsync_ReturnsTrue_WhenRecordExists()
    //     {
    //         // Arrange
    //         var connectionMock = new Mock<IDbConnection>();
    //         connectionMock.Setup(m => m.QuerySingleOrDefaultAsync<int>(
    //    It.IsAny<string>(),           // SQL query
    //    It.IsAny<object>(),           // Parameters
    //    null,                         // IDbTransaction (optional)
    //    null,                         // Command timeout (optional)
    //    null                          // Command type (optional)
    //)).ReturnsAsync(1);

    //         _dataAccessHelper.GetType()
    //             .GetProperty("DbConnection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
    //             ?.SetValue(_dataAccessHelper, connectionMock.Object);

    //         // Act
    //         var result = await _dataAccessHelper.ExistsAsync("users", "user_id", 1);

    //         // Assert
    //         Assert.True(result);
    //         connectionMock.Verify(m => m.QuerySingleOrDefaultAsync<int>(
    //             It.IsAny<string>(),
    //             It.IsAny<object>(),
    //             null,
    //             null,
    //             null), Times.Once());
    //     }

    //     //[Fact]
    //     //public async Task ExistsAsync_ReturnsFalse_WhenRecordDoesNotExist()
    //     //{
    //     //    // Arrange
    //     //    var connectionMock = new Mock<IDbConnection>();
    //     //    connectionMock.Setup(m => m.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), It.IsAny<object>()))
    //     //        .ReturnsAsync(0);

    //     //    _dataAccessHelper.GetType()
    //     //        .GetProperty("DbConnection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
    //     //        ?.SetValue(_dataAccessHelper, connectionMock.Object);

    //     //    // Act
    //     //    var result = await _dataAccessHelper.ExistsAsync("users", "user_id", 1);

    //     //    // Assert
    //     //    Assert.False(result);
    //     //    connectionMock.Verify(m => m.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), It.IsAny<object>()), Times.Once());
    //     //}

    //     //[Fact]
    //     //public async Task GetProductDetailsAsync_ReturnsProductDetails_WhenGrpcCallSucceeds()
    //     //{
    //     //    // Arrange
    //     //    var expectedResponse = new ProductDetailsResponse { ProductId = 1, Name = "Test Product" };
    //     //    _productDetailsClientMock.Setup(m => m.GetProductDetailsAsync(1))
    //     //        .ReturnsAsync(expectedResponse);

    //     //    // Act
    //     //    var result = await _dataAccessHelper.GetProductDetailsAsync(1);

    //     //    // Assert
    //     //    Assert.Equal(expectedResponse, result);
    //     //    _productDetailsClientMock.Verify(m => m.GetProductDetailsAsync(1), Times.Once());
    //     //}

    //     //[Fact]
    //     //public async Task GetProductDetailsAsync_ThrowsException_WhenGrpcCallFails()
    //     //{
    //     //    // Arrange
    //     //    _productDetailsClientMock.Setup(m => m.GetProductDetailsAsync(1))
    //     //        .ThrowsAsync(new Exception("gRPC error"));

    //     //    // Act & Assert
    //     //    var exception = await Assert.ThrowsAsync<Exception>(() => _dataAccessHelper.GetProductDetailsAsync(1));
    //     //    Assert.Equal("gRPC error", exception.Message);
    //     //    _productDetailsClientMock.Verify(m => m.GetProductDetailsAsync(1), Times.Once());
    //     //}

    //     //[Fact]
    //     //public async Task GetInactiveFlagFromGrpcAsync_ReturnsFlag_WhenGrpcCallSucceeds()
    //     //{
    //     //    // Arrange
    //     //    _inactiveFlagClientMock.Setup(m => m.GetInactiveFlagAsync(123))
    //     //        .ReturnsAsync(true);

    //     //    // Act
    //     //    var result = await _dataAccessHelper.GetInactiveFlagFromGrpcAsync(123);

    //     //    // Assert
    //     //    Assert.True(result);
    //     //    _inactiveFlagClientMock.Verify(m => m.GetInactiveFlagAsync(123), Times.Once());
    //     //}

    //     //[Fact]
    //     //public async Task CheckEmailExistsAsync_ReturnsResponse_WhenGrpcCallSucceeds()
    //     //{
    //     //    // Arrange
    //     //    var expectedResponse = new EmailResponse { Exists = true };
    //     //    _customerClientMock.Setup(m => m.CheckEmailExistsAsync("test@example.com"))
    //     //        .ReturnsAsync(expectedResponse);

    //     //    // Act
    //     //    var result = await _dataAccessHelper.CheckEmailExistsAsync("test@example.com");

    //     //    // Assert
    //     //    Assert.Equal(expectedResponse, result);
    //     //    Assert.True(result.Exists);
    //     //    _customerClientMock.Verify(m => m.CheckEmailExistsAsync("test@example.com"), Times.Once());
    //     //}

    //     //[Fact]
    //     //public async Task UpdateProductStockAsync_ReturnsTrue_WhenUpdateSucceeds()
    //     //{
    //     //    // Arrange
    //     //    var connectionMock = new Mock<NpgsqlConnection>();
    //     //    connectionMock.Setup(m => m.OpenAsync()).Returns(Task.CompletedTask);
    //     //    connectionMock.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
    //     //        .ReturnsAsync(1);

    //     //    // Use reflection or a factory to inject the connection if needed; for simplicity, we'll assume it works
    //     //    // Alternatively, refactor DataAccessHelper to accept a factory for testing.

    //     //    // Act
    //     //    var result = await _dataAccessHelper.UpdateProductStockAsync(1, 10);

    //     //    // Assert
    //     //    Assert.True(result);
    //     //}

    //     //[Fact]
    //     //public async Task UpdateProductStockByOrderedAsync_ReturnsTrue_WhenStockIsSufficient()
    //     //{
    //     //    // Arrange
    //     //    var connectionMock = new Mock<NpgsqlConnection>();
    //     //    connectionMock.Setup(m => m.OpenAsync()).Returns(Task.CompletedTask);
    //     //    connectionMock.Setup(m => m.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), It.IsAny<object>()))
    //     //        .ReturnsAsync(20); // Available stock
    //     //    connectionMock.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
    //     //        .ReturnsAsync(1);

    //     //    // Act
    //     //    var result = await _dataAccessHelper.UpdateProductStockByOrderedAsync(1, -10); // Reducing stock by 10

    //     //    // Assert
    //     //    Assert.True(result);
    //     //}

    //     //[Fact]
    //     //public async Task UpdateProductStockByOrderedAsync_ReturnsFalse_WhenStockIsInsufficient()
    //     //{
    //     //    // Arrange
    //     //    var connectionMock = new Mock<NpgsqlConnection>();
    //     //    connectionMock.Setup(m => m.OpenAsync()).Returns(Task.CompletedTask);
    //     //    connectionMock.Setup(m => m.QuerySingleOrDefaultAsync<int>(It.IsAny<string>(), It.IsAny<object>()))
    //     //        .ReturnsAsync(5); // Available stock
    //     //    connectionMock.Setup(m => m.ExecuteAsync(It.IsAny<string>(), It.IsAny<object>()))
    //     //        .ReturnsAsync(0);

    //     //    // Act
    //     //    var result = await _dataAccessHelper.UpdateProductStockByOrderedAsync(1, -10); // Reducing stock by 10

    //     //    // Assert
    //     //    Assert.False(result);
    //     //}
    // }
}
