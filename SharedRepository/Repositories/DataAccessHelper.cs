using System;
using System.Threading.Tasks;
using Dapper;
using GrpcClient;
using Npgsql;

namespace SharedRepository.Repositories
{
    public class DataAccessHelper : IDataAccessHelper
    {
        //private readonly string _dbconnection;
        private readonly string _dbconnection = "Host=dpg-csl1qfrv2p9s73ae0iag-a.oregon-postgres.render.com;Database=inventorymanagement_h8uy;Username=netconsumer;Password=UBmEj8MjJqg4zlimlXovbyt0bBDcrmiF";
        private readonly InactiveFlagClient _inactiveFlagClient;

        public DataAccessHelper(InactiveFlagClient inactiveFlagClient)
        {
            _inactiveFlagClient = inactiveFlagClient;
        }

        public async Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        {
            using var connection = new NpgsqlConnection(_dbconnection);
            await connection.OpenAsync();
            Console.WriteLine($"Connection opened: {connection}");

            var query = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";
            var exists = await connection.QuerySingleOrDefaultAsync<int>(query, new { IdValue = idValue });

            Console.WriteLine($"ExistsAsync result for {tableName}: {exists}");
            return exists > 0;
        }

        public async Task<(int ProductId, decimal Price, int Stock, decimal TaxPercentage)> GetProductDetailsAsync(int productId)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbconnection);
                await connection.OpenAsync();
                Console.WriteLine($"Connection opened: {connection}");

                var query = "SELECT product_id, price, stock, tax_percentage FROM products WHERE product_id = @ProductId";
                var result = await connection.QuerySingleOrDefaultAsync<(int ProductId, decimal Price, int Stock, decimal TaxPercentage)>(
                    query, new { ProductId = productId });

                return result == default ? (0, 0, 0, 0) : result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving product details: {ex.Message}");
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


        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbconnection);
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
                using var connection = new NpgsqlConnection(_dbconnection);
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
