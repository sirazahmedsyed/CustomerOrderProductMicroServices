using PurchaseService.API.Infrastructure.DTOs;

namespace PurchaseService.API.Infrastructure.Services
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseDTO>> GetAllPurchasesAsync();
        Task<(bool IsSuccess, PurchaseDTO Purchase, string Message)> AddPurchaseAsync(PurchaseDTO purchaseDto);
    }
}
