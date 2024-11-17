using System;
using System.Threading.Tasks;
using Dapper;
using Npgsql;

namespace SharedRepository.Repositories
{
    public interface IDataAccessHelper
    {
        Task<bool> ExistsAsync(string tableName, string idColumn, object idValue);
        Task<(int ProductId, decimal Price, int Stock, decimal TaxPercentage)> GetProductDetailsAsync(int productId);
        Task<bool> UpdateProductStockAsync(int productId, int quantity);
        Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity);

        Task<bool> GetInactiveFlagFromGrpcAsync(int userGroupNo);
    }
}