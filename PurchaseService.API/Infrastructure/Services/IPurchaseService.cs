using Microsoft.AspNetCore.Mvc;
using PurchaseService.API.Infrastructure.DTOs;

namespace PurchaseService.API.Infrastructure.Services
{
    public interface IPurchaseService
    {
        Task<PurchaseDTO> GetPurchaseByIdAsync(int purchaseId);
        Task<IEnumerable<PurchaseDTO>> GetAllPurchasesAsync();
        Task<IActionResult> AddPurchaseAsync(PurchaseDTO purchaseDto);
        Task<IActionResult> UpdatePurchaseAsync(PurchaseDTO updatedPurchaseDto);
        Task<IActionResult> DeletePurchaseAsync(int id);
    }
}
