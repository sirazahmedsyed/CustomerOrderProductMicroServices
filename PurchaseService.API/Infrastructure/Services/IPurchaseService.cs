using Microsoft.AspNetCore.Mvc;
using PurchaseService.API.Infrastructure.DTOs;
using PurchaseService.API.Infrastructure.Entities;

namespace PurchaseService.API.Infrastructure.Services
{
    public interface IPurchaseService
    {
        Task<PurchaseDTO> GetPurchaseByIdAsync(int purchaseId);
        Task<IEnumerable<PurchaseDTO>> GetAllPurchasesAsync();
        Task<(bool IsSuccess, PurchaseDTO Purchase, string Message)> AddPurchaseAsync(PurchaseDTO purchaseDto);
        Task<(bool IsSuccess, PurchaseDTO Purchase, string Message)> UpdatePurchaseAsync(PurchaseDTO updatedPurchaseDto);
        Task<(bool IsSuccess, string Message)> DeletePurchaseAsync(int id);
    }
}
