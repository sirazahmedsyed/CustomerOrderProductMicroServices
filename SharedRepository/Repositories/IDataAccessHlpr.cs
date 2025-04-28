using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public interface IDataAccessHlpr
    {
        Task<bool> ExistsAsync(string tableName, string idColumn, object idValue);
        //Task<bool> UpdateProductStockAsync(int productId, int quantity);
        //Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity);
    }
}

