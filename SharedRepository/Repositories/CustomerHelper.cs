using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public class CustomerHelper : ICusotmerHelper
    {
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
        public async Task<bool> CustomerExistsAsync(Guid customerId)
        {
            using var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");
            var customerQuery = $"SELECT 1 FROM customers WHERE customer_id = '{customerId}'";
            var customerExists = await connection.QuerySingleOrDefaultAsync<int>(customerQuery);
            Console.WriteLine($"CustomerExistsAsync result: {customerExists}");
            return customerExists > 0;
        }
    }
}