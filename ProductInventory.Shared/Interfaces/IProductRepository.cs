using ProductInventory.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductInventory.Shared.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> CreateProductAsync(Product product);
        Task<Product?> GetProductAsync(string id);
        Task<Product> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(string id);
        Task<int> GetStockLevelAsync(string productId);
        Task<IEnumerable<Product>> GetAllProductsAsync();
    }
}
