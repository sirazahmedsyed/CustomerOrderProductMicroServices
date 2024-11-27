using System;
using System.Threading.Tasks;
using Dapper;
using GrpcService;
using Npgsql;

namespace SharedRepository.Repositories
{
    public interface IDataAccessHelper
    {
        Task<bool> ExistsAsync(string tableName, string idColumn, object idValue);
        Task<ProductDetailsResponse> GetProductDetailsAsync(int productId);
        Task<bool> UpdateProductStockAsync(int productId, int quantity);
        Task<bool> UpdateProductStockByOrderedAsync(int productId, int quantity);

        Task<bool> GetInactiveFlagFromGrpcAsync(int userGroupNo);
        Task<bool> GetInactiveCustomerFlag(Guid CustomerId);
        Task<EmailResponse> CheckEmailExistsAsync(string email);
    }
}