using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Services;
using SharedRepository.Authorization;

namespace ProductService.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                _logger.LogInformation("Getting all products");
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("GetProductById/{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                _logger.LogInformation("Getting product with ID {ProductId}", id);
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", id);
                    return NotFound($"Product with ID {id} not found.");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost]
        [Route("CreateProduct")]
        [Authorize(Policy = Permissions.AddProducts)]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for product creation");
                return BadRequest(ModelState);
            }
            try
            {
                //return await _productService.AddProductAsync(productDto);
                var result = await _productService.AddProductAsync(productDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating product with ID {ProductId}", productDto.ProductId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for product update");
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _productService.UpdateProductAsync(productDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", productDto.ProductId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("DeleteProduct/{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

