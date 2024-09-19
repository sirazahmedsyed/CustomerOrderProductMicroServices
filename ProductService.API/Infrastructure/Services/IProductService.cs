using ProductService.API.Infrastructure.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.API.Infrastructure.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int id);
        Task<ProductDTO> AddProductAsync(ProductDTO productDto);
        Task<ProductDTO> UpdateProductAsync(ProductDTO productDto); 
        Task<bool> DeleteProductAsync(int id);
    }
}
