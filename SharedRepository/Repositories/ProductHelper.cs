using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public class ProductHelper : IProductHelper
    {
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
        public async Task<(int ProductId, decimal Price, int Stock, decimal TaxPercentage)> GetProductDetailsAsync(int productId)
        {
            try
            {
                using var connection = new NpgsqlConnection(dbconnection);
                connection.Open();
                Console.WriteLine($"connection opened : {connection}");

                var query = $"SELECT product_id, price, stock,tax_percentage FROM products WHERE product_id = {productId}";
                var result = await connection.QuerySingleOrDefaultAsync<(int ProductId, decimal Price, int Stock, decimal TaxPercentage)>(query);

                if (result == default)
                {
                    return (0, 0, 0,0);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving product details: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        {
            try
            {
                using var connection = new NpgsqlConnection(dbconnection);
                connection.Open();
                Console.WriteLine($"connection opened : {connection}");

                var query = $"UPDATE products SET stock = stock + {quantity} WHERE product_id = {productId}";
                var result = await connection.ExecuteAsync(query, new { Quantity = quantity, ProductId = productId });

                if(result >0) { return true; }
                Console.WriteLine($"Rows affected: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }
            return false;
        }

        public async Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity)
        {
            try
            {
                using var connection = new NpgsqlConnection(dbconnection);
                connection.Open();

                var stockQuery = $"SELECT stock FROM products WHERE product_id = {productId}";
                var availableStock = await connection.QuerySingleOrDefaultAsync<int>(stockQuery);

                if (availableStock + quantity < 0) 
                {
                    Console.WriteLine($"Insufficient stock for product ID {productId}. Available: {availableStock}, Requested: {-quantity}");
                    return false; 
                }

                var updateQuery = $"UPDATE products SET stock = stock + {quantity} WHERE product_id = {productId}";
                var result = await connection.ExecuteAsync(updateQuery, new { Quantity = quantity, ProductId = productId });

                if (result > 0)
                {
                    Console.WriteLine($"Stock updated for product ID {productId}. Updated stock: {availableStock + quantity}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product stock: {ex.Message}");
                throw;
            }

            return false;
        }
    }
}