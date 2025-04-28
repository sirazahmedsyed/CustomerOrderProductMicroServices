using Dapper;
using GrpcClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Test
{
    public class TestableDataAccessHelper : SharedRepository.Repositories.DataAccessHelper
    {
        //private readonly IDbConnection _testDbConnection;

        //public TestableDataAccessHelper(
        //    IDbConnection testDbConnection,
        //    InactiveFlagClient inactiveFlagClient,
        //    ProductDetailsClient productDetailsClient,
        //    CustomerClient customerClient)
        //    : base(inactiveFlagClient, productDetailsClient, customerClient)
        //{
        //    _testDbConnection = testDbConnection;
        //}

        //// Override the method we want to test with a mock connection
        //public new async Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        //{
        //    var query = $"SELECT 1 FROM {tableName} WHERE {idColumn} = @IdValue";
        //    var exists = await _testDbConnection.QuerySingleOrDefaultAsync<int>(query, new { IdValue = idValue });
        //    return exists > 0;
        //}

        private readonly bool _existsResult;

        public TestableDataAccessHelper(
            bool existsResult,
            InactiveFlagClient inactiveFlagClient,
            ProductDetailsClient productDetailsClient,
            CustomerClient customerClient)
            : base(inactiveFlagClient, productDetailsClient, customerClient)
        {
            _existsResult = existsResult;
        }

        // Override the method to return our predetermined result
        public new Task<bool> ExistsAsync(string tableName, string idColumn, object idValue)
        {
            // Record was called for verification
            CalledWithParameters = (tableName, idColumn, idValue);

            return Task.FromResult(_existsResult);
        }

        // Expose parameters for verification
        public (string tableName, string idColumn, object idValue)? CalledWithParameters { get; private set; }
    }
}
