using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ProductService.API.Infrastructure.Entities;

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
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for product creation");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, createdProduct, message) = await _productService.AddProductAsync(productDto);

                if (!isSuccess)
                {
                    _logger.LogWarning("Duplicate product with ID {ProductId} not allowed", productDto.ProductId);
                    return BadRequest(new { message, productId = productDto.ProductId });
                }

                _logger.LogInformation("Product with ID {ProductId} created", createdProduct.ProductId);
                return Ok(createdProduct);
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
                var updatedProduct = await _productService.UpdateProductAsync(productDto);
                if (updatedProduct == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", productDto.ProductId);
                    return NotFound($"Product with ID {productDto.ProductId} not found.");
                }

                _logger.LogInformation("Product with ID {ProductId} updated", productDto.ProductId);
                return Ok(updatedProduct);
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
                _logger.LogInformation("Deleting product with ID {ProductId}", id);
                var deletedProduct = await _productService.DeleteProductAsync(id);
                if (!deletedProduct)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", id);
                    return NotFound($"Product with ID {id} not found.");
                }

                _logger.LogInformation("Product with ID {ProductId} deleted", id);
                return Ok(deletedProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

