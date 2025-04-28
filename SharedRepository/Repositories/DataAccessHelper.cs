using Dapper;
using GrpcClient;
using GrpcService;
using Npgsql;
using System.Data;

namespace SharedRepository.Repositories
{
    public class DataAccessHelper : IDataAccessHelper
    {
        protected IDbConnection DbConnection { get; set; } = default!;
        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";
        private readonly InactiveFlagClient _inactiveFlagClient;
        private readonly ProductDetailsClient _productDetailsClient;
        private readonly CustomerClient _customerClient;

        public DataAccessHelper(InactiveFlagClient inactiveFlagClient, ProductDetailsClient productDetailsClient, CustomerClient customerClient)
        {
            _inactiveFlagClient = inactiveFlagClient;
            _productDetailsClient = productDetailsClient;
            _customerClient = customerClient;
        }
        public async Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        {
            DbConnection = new NpgsqlConnection(dbconnection);
            Console.WriteLine($"Connection opened: {DbConnection}");
            var query = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";
            var exists = await DbConnection.QuerySingleOrDefaultAsync<int>(query, new { IdValue = idValue });

            Console.WriteLine($"ExistsAsync result for {tableName}: {exists}");
            return exists > 0;
        }
        


        public async Task<ProductDetailsResponse> GetProductDetailsAsync(int productId)
        {
            try
            {
                // Call the gRPC client to get product details
                var productDetails = await _productDetailsClient.GetProductDetailsAsync(productId);
                Console.WriteLine($"Retrieved product details for product {productId}: {productDetails}");
                return productDetails;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting product details from gRPC service: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> GetInactiveFlagFromGrpcAsync(int userGroupNo)
        {
            try
            {
                // Call the gRPC client to get inactive flag
                var inactiveFlag = await _inactiveFlagClient.GetInactiveFlagAsync(userGroupNo);
                Console.WriteLine($"Retrieved inactive flag for user group {userGroupNo}: {inactiveFlag}");
                return inactiveFlag;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inactive flag from gRPC service: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> GetInactiveCustomerFlag(Guid CustomerId)
        {
            try
            {
                // Call the gRPC client to get inactive flag
                var inactiveFlag = await _inactiveFlagClient.GetInactiveCustomerFlagAsync(CustomerId);
                Console.WriteLine($"Retrieved inactive flag for customer {CustomerId}: {inactiveFlag}");
                return inactiveFlag;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inactive flag from gRPC service: {ex.Message}");
                throw;
            }
        }


        public async Task<EmailResponse> CheckEmailExistsAsync(string email)
        { 
            try 
            { 
                   var response = await _customerClient.CheckEmailExistsAsync(email); 
                   Console.WriteLine($"Email check completed: {response}");
                return response; 
            } 
            catch (Exception ex) 
            { 
                Console.WriteLine($"Error checking email via gRPC service: {ex.Message}"); 
                throw; 
            }
        }


        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            try
            {
                using var connection = new NpgsqlConnection(dbconnection);
                await connection.OpenAsync();
                Console.WriteLine($"Connection opened: {connection}");

                var query = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";
                var result = await connection.ExecuteAsync(query, new { Quantity = quantity, ProductId = productId });

                Console.WriteLine($"Rows affected: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity)
        {
            try
            {
                using var connection = new NpgsqlConnection(dbconnection);
                await connection.OpenAsync();

                var stockQuery = "SELECT stock FROM products WHERE product_id = @ProductId";
                var availableStock = await connection.QuerySingleOrDefaultAsync<int>(stockQuery, new { ProductId = productId });

                if (availableStock + quantity < 0)
                {
                    Console.WriteLine($"Insufficient stock for product ID {productId}. Available: {availableStock}, Requested: {-quantity}");
                    return false;
                }

                var updateQuery = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";
                var result = await connection.ExecuteAsync(updateQuery, new { Quantity = quantity, ProductId = productId });

                if (result > 0)
                {
                    Console.WriteLine($"Stock updated for product ID {productId}. Updated stock: {availableStock + quantity}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }
        }
    }
}
