using Dapper;
using GrpcService;
using Npgsql;

namespace GrpcService.Repository
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ILogger<CustomerRepository> _logger;
        private readonly string _dbConnection;

        public CustomerRepository(ILogger<CustomerRepository> logger)
        {
            _logger = logger;
            _dbConnection = "Host=dpg-ctaj11q3esus739aqeb0-a.oregon-postgres.render.com;Database=inventorymanagement_m3a1;Username=netconsumer;Password=y5oyt0LjENzsldOuO4zZ3mB2WbeM2ohw";
        }

        public async Task<EmailResponse> CheckEmailExistsAsync(EmailRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbConnection);
                await connection.OpenAsync();
                _logger.LogInformation($"Database connection opened for checking email: {request.Email}");

                //var emailExists = await connection.QuerySingleOrDefaultAsync<bool>($"select email from customers where email = {request.Email}");
                var query = "select exists (select 1 from customers where email = '{request.Email}')"; 
                var emailExists = await connection.QuerySingleOrDefaultAsync<bool>(query, new { Email = request.Email });
                _logger.LogInformation($"Email check completed for: {request.Email}, exists: {emailExists}");

                return new EmailResponse 
                { 
                    EmailExists = emailExists 
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email");
                throw;
            }
        }
    }
}
