using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.UnitOfWork;
using SharedRepository.RabbitMQMessageBroker.Interfaces;
using SharedRepository.RabbitMQMessageBroker.Settings;

namespace ProductService.API.Infrastructure.Services
{
    public class ProductServices : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMessagePublisher<ProductDTO> _messagePublisher;
        private readonly ILogger<ProductServices> _logger;
        private readonly RabbitMQSettings _settings;
        private readonly string dbconnection = "Host=dpg-ctuh03lds78s73fntmag-a.oregon-postgres.render.com;Database=order_management_db;Username=netconsumer;Password=wv5ZjPAcJY8ICgPJF0PZUV86qdKx2r7d";
        public ProductServices(IUnitOfWork unitOfWork, IMapper mapper, IMessagePublisher<ProductDTO> messagePublisher, ILogger<ProductServices> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _settings = RabbitMQConfigurations.DefaultSettings;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<IActionResult> AddProductAsync(ProductDTO productDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();
            Console.WriteLine($"connection opened : {connection}");

            var existingProductByName = await connection.QuerySingleOrDefaultAsync<string>($"select name from products where name = '{productDto.Name}'");

            if (existingProductByName != null)
            {
                return new BadRequestObjectResult(new { message = $"Duplicate product not allowed for this product {productDto.Name}" });
            }

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            try
            {
                // Publish the OrderCreated event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<ProductDTO>(product), _settings.Queues.ProductCreated);
                _logger.LogInformation($"ProductCreated event published successfully for the {_settings.Queues.ProductCreated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing ProductCreated event to RabbitMQ");
                throw;
            }

            return new OkObjectResult(new
            {
                message = "Product created successfully",
                product = _mapper.Map<ProductDTO>(product)
            });
        }

        public async Task<IActionResult> UpdateProductAsync(ProductDTO productDto)
        {

            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var existingProduct = await connection.QuerySingleOrDefaultAsync<Product>($"SELECT * FROM products WHERE product_id = '{productDto.ProductId}'");

            //var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
            { 
                return new BadRequestObjectResult(new { message = $"Product is not availble for this {productDto.ProductId} productId" });
            }

            _mapper.Map(productDto, existingProduct); 
            _unitOfWork.Repository<Product>().Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            try
            {
                // Publish the OrderCreated event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<ProductDTO>(existingProduct), _settings.Queues.ProductUpdated);
                _logger.LogInformation($"ProductUpdated event published successfully for the {_settings.Queues.ProductUpdated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing ProductUpdated event to RabbitMQ");
                throw;
            }

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
                return new BadRequestObjectResult(new { message = $"Product with ID {id} not found." });
            }

            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.CompleteAsync();

            try
            {
                // Publish the OrderCreated event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<ProductDTO>(product), _settings.Queues.ProductDeleted);
                _logger.LogInformation($"ProductDeleted event published successfully for the {_settings.Queues.ProductDeleted}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing ProductDeleted event to RabbitMQ");
                throw;
            }

            return new OkObjectResult(new { message = "Product deleted successfully." });
        }
    }
}
