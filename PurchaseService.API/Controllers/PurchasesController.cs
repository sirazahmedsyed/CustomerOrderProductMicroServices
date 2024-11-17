using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseService.API.Infrastructure.DTOs;
using PurchaseService.API.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace PurchaseService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(IPurchaseService purchaseService, ILogger<PurchasesController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetAllPurchases")]
        public async Task<IActionResult> GetAllPurchases()
        {
            try
            {
                _logger.LogInformation("Getting all purchases");
                var purchases = await _purchaseService.GetAllPurchasesAsync();
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all purchases");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("CreatePurchase")]
        public async Task<IActionResult> CreatePurchase([FromBody] PurchaseDTO purchaseDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for purchase creation");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, createdPurchase, message) = await _purchaseService.AddPurchaseAsync(purchaseDto);

                if (!isSuccess)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message = message });
                }

                _logger.LogInformation("Purchase created successfully");
                return Ok(createdPurchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating purchase");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut]
        [Route("UpdatePurchase")]
        public async Task<IActionResult> UpdatePurchase([FromBody] PurchaseDTO purchaseDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for purchase update");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, updatedPurchase, message) = await _purchaseService.UpdatePurchaseAsync(purchaseDto);

                if (!isSuccess)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message = message });
                }

                _logger.LogInformation("Purchase updated successfully");
                return Ok(updatedPurchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating purchase");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("DeletePurchase/{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            try
            {
                var (isSuccess, message) = await _purchaseService.DeletePurchaseAsync(id);

                if (!isSuccess)
                {
                    _logger.LogWarning(message);
                    return NotFound(new { message = message });
                }

                _logger.LogInformation("Purchase deleted successfully");
                return Ok(new { message = message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting purchase");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
