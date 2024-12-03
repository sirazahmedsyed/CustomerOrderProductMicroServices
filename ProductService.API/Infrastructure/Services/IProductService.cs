using Microsoft.AspNetCore.Mvc;
using ProductService.API.Infrastructure.DTOs;

namespace ProductService.API.Infrastructure.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int id);
        //Task<ProductDTO> AddProductAsync(ProductDTO productDto);
        Task<IActionResult> AddProductAsync(ProductDTO productDto);
        Task<IActionResult> UpdateProductAsync(ProductDTO productDto); 
        Task<IActionResult> DeleteProductAsync(int id);
    }
}
