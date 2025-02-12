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
            _dbConnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";
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
