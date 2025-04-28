using Dapper;
using Npgsql;
using SharedRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{

    public class DataAccessHlpr : IDataAccessHlpr
    {
        //private ProductDbContext _context;

        //public DataAccessHlpr(ProductDbContext context)
        //{
        //    _context = context;
        //}

        //public async Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        //{
        //    using var connection = _context.CreateConnection();
        //    var query = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";

        //    try
        //    {
        //        var exists = await connection.QuerySingleOrDefaultAsync<int>(query, new { IdValue = idValue });
        //        return exists > 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle the exception appropriately, log it, or rethrow.
        //        Console.WriteLine($"Exception in ExistsAsync: {ex.Message}");
        //        return false; // Or re-throw the exception if appropriate
        //    }
        //}
        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20;Database=management_db;Username=netconsumer;Password=6j9LZ";
        private IDbConnection DbConnection { get; set; } = default!;

        private readonly IDapperHelper dapperHelper;
        public DataAccessHlpr(string dbConnection, IDapperHelper dapperHelper)
        {
            dbconnection = dbConnection;
            this.dapperHelper = dapperHelper;
        }
        public async Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        {
            //using var connection = new NpgsqlConnection(dbconnection);
            DbConnection = new NpgsqlConnection(dbconnection);
            var query = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";
            var exists = await dapperHelper.QuerySingleOrDefaultAsync<int>(DbConnection, query, new { IdValue = idValue });
            return exists > 0;
        }

        //public async Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        //{
        //    DbConnection = new NpgsqlConnection(dbconnection);
        //    Console.WriteLine($"Connection opened: {DbConnection}");
        //    var query = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";
        //    var exists = await DbConnection.QuerySingleOrDefaultAsync<int>(query, new { IdValue = idValue });

        //    Console.WriteLine($"ExistsAsync result for {tableName}: {exists}");
        //    return exists > 0;
        //}
        //public async Task<bool> UpdateProductStockAsync(int productId, int quantity)
        //{
        //    DbConnection = new NpgsqlConnection(dbconnection);
        //    var query = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";
        //    var result = await DbConnection.ExecuteAsync(query, new { Quantity = quantity, ProductId = productId });
        //    return result > 0;
        //}

        //public async Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity)
        //{
        //    DbConnection = new NpgsqlConnection(dbconnection);
        //    var stockQuery = "SELECT stock FROM products WHERE product_id = @ProductId";
        //    var availableStock = await DbConnection.QuerySingleOrDefaultAsync<int>(stockQuery, new { ProductId = productId });

        //    if (availableStock + quantity < 0)
        //    {
        //        Console.WriteLine($"Insufficient stock. Available: {availableStock}, Requested: {-quantity}");
        //        return false;
        //    }

        //    var updateQuery = "UPDATE products SET stock = stock + @Quantity WHERE product_id = @ProductId";
        //    var result = await DbConnection.ExecuteAsync(updateQuery, new { Quantity = quantity, ProductId = productId });
        //    return result > 0;
        //}
    }

}