using ProductInventory.Shared.Entities;
using ProductInventory.Shared.Interfaces;

namespace ProductInventory.Server.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly Dictionary<string, Product> _products = new();
        private readonly object _lock = new();

        public Task<Product> CreateProductAsync(Product product)
        {
            lock (_lock)
            {
                if (_products.ContainsKey(product.Id))
                    throw new InvalidOperationException("Product already exists");

                _products[product.Id] = product;
                return Task.FromResult(product);
            }
        }

        public Task<Product?> GetProductAsync(string id)
        {
            lock (_lock)
            {
                return Task.FromResult(_products.GetValueOrDefault(id));
            }
        }

        public Task<Product> UpdateProductAsync(Product product)
        {
            lock (_lock)
            {
                if (!_products.ContainsKey(product.Id))
                    throw new KeyNotFoundException("Product not found");

                product.LastUpdated = DateTime.UtcNow;
                _products[product.Id] = product;
                return Task.FromResult(product);
            }
        }

        public Task<bool> DeleteProductAsync(string id)
        {
            lock (_lock)
            {
                return Task.FromResult(_products.Remove(id));
            }
        }

        public Task<int> GetStockLevelAsync(string productId)
        {
            lock (_lock)
            {
                return Task.FromResult(_products.GetValueOrDefault(productId)?.Stock ?? 0);
            }
        }

        public Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_products.Values.AsEnumerable());
            }
        }
    }
}
