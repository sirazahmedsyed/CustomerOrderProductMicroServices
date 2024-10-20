using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public class OrderHelper : IOrderHelper
    {
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
        public async Task<bool> OrderExistsAsync(Guid order_id)
        {
            using var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var orderQuery = $"SELECT 1 FROM orders WHERE order_id = '{order_id}'";
            var orderExists = await connection.QuerySingleOrDefaultAsync<int>(orderQuery);

            Console.WriteLine($"OrderExistsAsync result: {orderExists}");
            return orderExists > 0;
        }
    }
}