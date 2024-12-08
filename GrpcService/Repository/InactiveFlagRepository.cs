using Dapper;
using GrpcService;
using Npgsql;

namespace GrpcService.Repository
{
    public class InactiveFlagRepository : IInactiveFlagRepository
    {
        private readonly ILogger<InactiveFlagRepository> _logger;
        private readonly string _dbConnection;

        public InactiveFlagRepository(ILogger<InactiveFlagRepository> logger)
        {
            _logger = logger;
            _dbConnection = "Host=dpg-ctaj11q3esus739aqeb0-a.oregon-postgres.render.com;Database=inventorymanagement_m3a1;Username=netconsumer;Password=y5oyt0LjENzsldOuO4zZ3mB2WbeM2ohw";
        }

        public async Task<InactiveFlagResponse> GetInactiveFlagAsync(InactiveFlagRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbConnection);
                await connection.OpenAsync();
                _logger.LogInformation($"Database connection opened for user_group_no: {request.UserGroupNo}");
                
                var inactiveFlag = await connection.QuerySingleOrDefaultAsync<bool>($"SELECT inactive_flag FROM user_groups WHERE user_group_no = {request.UserGroupNo}");

                _logger.LogInformation($"Retrieved inactive_flag: {inactiveFlag} for user_group_no: {request.UserGroupNo}");

                return new InactiveFlagResponse
                {
                    InactiveFlag = inactiveFlag
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving inactive_flag for user_group_no: {request.UserGroupNo}");
                throw;
            }
        }

        public async Task<InactiveCustomerFlagResponse> GetInactiveCustomerFlagAsync(InactiveCustomerFlagRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbConnection);
                await connection.OpenAsync();
                _logger.LogInformation($"Database connection opened for Customer: {request.CustomerId}");
                
                var inactiveFlag = await connection.QuerySingleOrDefaultAsync<bool>($"select inactive_flag from customers where customer_id = '{new Guid(request.CustomerId.ToByteArray())}'");

                _logger.LogInformation($"Retrieved inactive_flag: {inactiveFlag} for CustomerId: {request.CustomerId}");

                return new InactiveCustomerFlagResponse
                {

                    InactiveFlag = inactiveFlag
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving inactive_flag for customerId: {request.CustomerId}");
                throw;
            }
        }

    }
}
