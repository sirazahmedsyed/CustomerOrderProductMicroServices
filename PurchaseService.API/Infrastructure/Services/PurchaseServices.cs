using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PurchaseService.API.Infrastructure.DTOs;
using PurchaseService.API.Infrastructure.Entities;
using PurchaseService.API.Infrastructure.UnitOfWork;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.RedisCache;
using SharedRepository.Repositories;

namespace PurchaseService.API.Infrastructure.Services
{
    public class PurchaseServices : IPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseServices> _logger;
        private readonly ICacheService _cacheService;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string ALL_PURCHASES_KEY = "purchases:all";
        private const string PURCHASE_KEY_PREFIX = "purchase:";

        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";

        public PurchaseServices(IUnitOfWork unitOfWork, IDataAccessHelper dataAccessHelper, IMapper mapper,
            ILogger<PurchaseServices> logger, ICacheService cacheService, IRabbitMQHelper rabbitMQHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _dataAccessHelper = dataAccessHelper;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
            _rabbitMQHelper = rabbitMQHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<PurchaseDTO> GetPurchaseByIdAsync(int id)
        {
            var cacheKey = $"{PURCHASE_KEY_PREFIX}{id}";

            var cachedPurchase = await _cacheService.GetAsync<PurchaseDTO>(cacheKey);
            if (cachedPurchase != null)
            {
                _logger.LogInformation($"Retrieved purchase {id} from cache");
                return cachedPurchase;
            }

            var purchase = await _unitOfWork.Repository<Purchase>().GetByIdAsync(id);
            if (purchase == null) return null;

            await _cacheService.SetAsync(cacheKey, _mapper.Map<PurchaseDTO>(purchase), TimeSpan.FromMinutes(30));

            return _mapper.Map<PurchaseDTO>(purchase);
        }

        public async Task<IEnumerable<PurchaseDTO>> GetAllPurchasesAsync()
        {
            var cachedPurchases = await _cacheService.GetAsync<IEnumerable<PurchaseDTO>>(ALL_PURCHASES_KEY);
            if (cachedPurchases != null)
            {
                _logger.LogInformation("Retrieved purchases from cache");
                return cachedPurchases;
            }

            var purchases = await _unitOfWork.Repository<Purchase>().GetAllAsync();

            await _cacheService.SetAsync(ALL_PURCHASES_KEY, _mapper.Map<IEnumerable<PurchaseDTO>>(purchases), TimeSpan.FromMinutes(5));

            return _mapper.Map<IEnumerable<PurchaseDTO>>(purchases);
        }

        public async Task<IActionResult> AddPurchaseAsync(PurchaseDTO purchaseDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var purchaseExists = await _dataAccessHelper.ExistsAsync("purchases", "purchase_order_no", purchaseDto.PurchaseOrderNo);
            if (purchaseExists)
            {
                return new BadRequestObjectResult(new { message = $"Duplicate purchase not allowed for this {purchaseDto.PurchaseOrderNo}." });
            }

            var productDetails = await _dataAccessHelper.GetProductDetailsAsync(purchaseDto.ProductId);
            if (productDetails.ProductId == 0)
            {
                return new BadRequestObjectResult(new { message = $"Product with ID {purchaseDto.ProductId} does not exist." });
            }

            var stockUpdated = await _dataAccessHelper.UpdateProductStockAsync(purchaseDto.ProductId, purchaseDto.Quantity);
            if (!stockUpdated)
            {
                return new BadRequestObjectResult(new { message = $"Failed to update product stock." });
            }

            var purchase = _mapper.Map<Purchase>(purchaseDto);
            await _unitOfWork.Repository<Purchase>().AddAsync(purchase);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(ALL_PURCHASES_KEY);
            await _cacheService.SetAsync($"{PURCHASE_KEY_PREFIX}{purchase.PurchaseId}", _mapper.Map<PurchaseDTO>(purchase));

            await SendAuditMessage(1, purchase.PurchaseId, "Created");

            return new OkObjectResult(new { message = "Purchase created successfully", Purchase = _mapper.Map<PurchaseDTO>(purchase) });
        }

        public async Task<IActionResult> UpdatePurchaseAsync(PurchaseDTO updatedPurchaseDto)
        {
            if (!await _dataAccessHelper.ExistsAsync("purchases", "purchase_order_no", updatedPurchaseDto.PurchaseOrderNo))
            {
                return new NotFoundObjectResult(new { message = $"Purchase with ID {updatedPurchaseDto.PurchaseId} not found." });
            }

            var productDetails = await _dataAccessHelper.GetProductDetailsAsync(updatedPurchaseDto.ProductId);
            if (productDetails.ProductId == default)
            {
                return new BadRequestObjectResult(new { message = $"Product with ID {updatedPurchaseDto.ProductId} does not exist." });
            }

            var existingPurchase = await _unitOfWork.Repository<Purchase>().GetByIdAsync(updatedPurchaseDto.PurchaseOrderNo);
            int quantityDifference = updatedPurchaseDto.Quantity - existingPurchase.Quantity;

            var stockUpdated = await _dataAccessHelper.UpdateProductStockAsync(updatedPurchaseDto.ProductId, quantityDifference);
            if (!stockUpdated)
            {
                return new BadRequestObjectResult(new { message = "Failed to update product stock." });
            }

            _mapper.Map(updatedPurchaseDto, existingPurchase);
            _unitOfWork.Repository<Purchase>().Update(existingPurchase);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{PURCHASE_KEY_PREFIX}{updatedPurchaseDto.PurchaseId}");
            await _cacheService.RemoveAsync(ALL_PURCHASES_KEY);

            await SendAuditMessage(2, existingPurchase.PurchaseId, "Updated");

            return new OkObjectResult(new
            {
                message = "Purchase updated successfully.",
                purchase = _mapper.Map<PurchaseDTO>(existingPurchase)
            });
        }

        public async Task<IActionResult> DeletePurchaseAsync(int purchaseId)
        {
            var existingPurchase = await _dataAccessHelper.ExistsAsync("purchases", "purchase_id", purchaseId);
            if (!existingPurchase)
            {
                return new NotFoundObjectResult(new { message = $"Purchase with ID {purchaseId} not found." });
            }

            var purchase = await _unitOfWork.Repository<Purchase>().GetByIdAsync(purchaseId);

            var stockUpdated = await _dataAccessHelper.UpdateProductStockAsync(purchase.ProductId, -purchase.Quantity);
            if (!stockUpdated)
            {
                return new BadRequestObjectResult(new { message = "Failed to update product stock." });
            }

            _unitOfWork.Repository<Purchase>().Remove(purchase);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{PURCHASE_KEY_PREFIX}{purchaseId}");
            await _cacheService.RemoveAsync(ALL_PURCHASES_KEY);

            await SendAuditMessage(3, purchaseId, "Deleted");

            return new OkObjectResult(new { message = "Purchase deleted successfully." });
        }

        private async Task SendAuditMessage(int operationType, int purchaseId, string action)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var auditMessageDto = new AuditMessageDto
            {
                OprtnTyp = operationType,
                UsrNm = username,
                UsrNo = 1,
                LogDsc = new List<string> { $"{action} By {username} {DateTime.UtcNow:ddd MMM dd HH:mm:ss 'UTC' yyyy}" },
                LogTyp = 1,
                LogDate = DateTime.UtcNow,
                ScreenName = "PurchasesController",
                ObjectName = "purchase",
                ScreenPk = new Guid(BitConverter.GetBytes(purchaseId).Concat(new byte[12]).ToArray())
            };
            await _rabbitMQHelper.AuditResAsync(auditMessageDto);
        }
    }
}