using Dapper;
using Npgsql;

namespace GrpcService.Repository
{
    public class ProductDetailsRepository : IProductDetailsRepository
    {
        private readonly ILogger<ProductDetailsRepository> _logger;
        private readonly string _dbConnection;

        public ProductDetailsRepository(ILogger<ProductDetailsRepository> logger)
        {
            _logger = logger;
            _dbConnection = "Host=dpg-ctaj11q3esus739aqeb0-a.oregon-postgres.render.com;Database=inventorymanagement_m3a1;Username=netconsumer;Password=y5oyt0LjENzsldOuO4zZ3mB2WbeM2ohw";
        }

        public async Task<ProductDetailsResponse> GetProductDetailsAsync(ProductDetailsRequest request)
        {
            try
            {
                using var connection = new NpgsqlConnection(_dbConnection);
                await connection.OpenAsync();
                _logger.LogInformation($"Database connection opened for Product: {request.ProductId}");

                //var query = "select product_id, price, stock, tax_percentage from products where product_id = @ProductId";
                var query = "SELECT product_id AS ProductId, price AS Price, stock AS Stock, tax_percentage AS TaxPercentage FROM products WHERE product_id = @ProductId";
                var result = await connection.QuerySingleOrDefaultAsync<ProductDetailsResponse>(query, new { ProductId = request.ProductId });

                if (result == null)
                {
                    return new ProductDetailsResponse();
                }

                _logger.LogInformation($"Retrieved product details for ProductId: {request.ProductId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product details for ProductId: {request.ProductId}");
                throw;
            }
        }
    }
}
