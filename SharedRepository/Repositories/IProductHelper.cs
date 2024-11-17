using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public interface IProductHelper
    {
        Task<(int ProductId, decimal Price, int Stock,decimal TaxPercentage)> GetProductDetailsAsync(int productId);
        Task<bool> UpdateProductStockAsync(int productId, int quantity);
        Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity);
        //Task<bool> ProudctExistsAsync(Guid product_id);
    }
}
