using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.UnitOfWork;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.RedisCache;

namespace ProductService.API.Infrastructure.Services
{
    public class ProductServices : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductServices> _logger;
        private readonly ICacheService _cacheService;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";

        private const string ALL_PRODUCTS_KEY = "products:all";
        private const string PRODUCT_KEY_PREFIX = "product:";

        public ProductServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductServices> logger, ICacheService cacheService, IRabbitMQHelper rabbitMQHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
            _rabbitMQHelper = rabbitMQHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var cachedProducts = await _cacheService.GetAsync<IEnumerable<ProductDTO>>(ALL_PRODUCTS_KEY);
            if (cachedProducts != null)
            {
                _logger.LogInformation("Retrieved products from cache");
                return cachedProducts;
            }

            var products = await _unitOfWork.Repository<Product>().GetAllAsync();

            await _cacheService.SetAsync(ALL_PRODUCTS_KEY, _mapper.Map<IEnumerable<ProductDTO>>(products), TimeSpan.FromMinutes(5));

            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var cacheKey = $"{PRODUCT_KEY_PREFIX}{id}";

            var cachedProduct = await _cacheService.GetAsync<ProductDTO>(cacheKey);
            if (cachedProduct != null)
            {
                _logger.LogInformation($"Retrieved product {id} from cache");
                return cachedProduct;
            }

            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return null;

            await _cacheService.SetAsync(cacheKey, _mapper.Map<ProductDTO>(product), TimeSpan.FromMinutes(30));

            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<IActionResult> AddProductAsync(ProductDTO productDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var existingProductByName = await connection.QuerySingleOrDefaultAsync<string>($"select name from products where name = '{productDto.Name}'");

            if (existingProductByName != null)
            {
                return new BadRequestObjectResult(new
                {
                    message = $"Duplicate product not allowed for this product {productDto.Name}"
                });
            }

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(ALL_PRODUCTS_KEY);
            await _cacheService.SetAsync($"{PRODUCT_KEY_PREFIX}{product.ProductId}", _mapper.Map<ProductDTO>(product));

            await SendAuditMessage(1, product.ProductId, "Created");

            return new OkObjectResult(new
            {
                message = "Product created successfully",
                product = _mapper.Map<ProductDTO>(product)
            });
        }

        public async Task<IActionResult> UpdateProductAsync(ProductDTO productDto)
        {
            var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = $"Product is not available for this {productDto.ProductId} productId"
                });
            }

            _mapper.Map(productDto, existingProduct);
            _unitOfWork.Repository<Product>().Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{PRODUCT_KEY_PREFIX}{productDto.ProductId}");
            await _cacheService.RemoveAsync(ALL_PRODUCTS_KEY);

            await SendAuditMessage(2, productDto.ProductId, "Updated");

            return new OkObjectResult(new
            {
                message = "Product Updated successfully",
                product = _mapper.Map<ProductDTO>(existingProduct)
            });
        }

        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return new BadRequestObjectResult(new
                {
                    message = $"Product with ID {id} not found."
                });
            }

            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{PRODUCT_KEY_PREFIX}{id}");
            await _cacheService.RemoveAsync(ALL_PRODUCTS_KEY);

            await SendAuditMessage(3, id, "Deleted");

            return new OkObjectResult(new { message = "Product deleted successfully." });
        }

        private async Task SendAuditMessage(int operationType, int productId, string action)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var auditMessageDto = new AuditMessageDto
            {
                OprtnTyp = operationType,
                UsrNm = username,
                UsrNo = 1,
                LogDsc = new List<string> { $"{action} By {username} {DateTime.UtcNow.ToString("ddd MMM dd HH:mm:ss 'UTC' yyyy")}" },
                LogTyp = 1,
                LogDate = DateTime.UtcNow,
                ScreenName = "ProductsController",
                ObjectName = "product",
                ScreenPk = new Guid(BitConverter.GetBytes(productId).Concat(new byte[12]).ToArray())
            };
            await _rabbitMQHelper.AuditResAsync(auditMessageDto);
        }
    }
}
