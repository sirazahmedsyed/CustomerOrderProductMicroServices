using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PurchaseService.API.Infrastructure.DTOs;
using PurchaseService.API.Infrastructure.Entities;
using PurchaseService.API.Infrastructure.UnitOfWork;
using SharedRepository.RabbitMQMessageBroker.Interfaces;
using SharedRepository.RabbitMQMessageBroker.Settings;
using SharedRepository.Repositories;

namespace PurchaseService.API.Infrastructure.Services
{
    public class PurchaseServices : IPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IMapper _mapper;
        private readonly IMessagePublisher<PurchaseDTO> _messagePublisher;
        private readonly ILogger<PurchaseServices> _logger;
        private readonly RabbitMQSettings _settings;

        private readonly string dbconnection = "Host=dpg-ctuh03lds78s73fntmag-a.oregon-postgres.render.com;Database=order_management_db;Username=netconsumer;Password=wv5ZjPAcJY8ICgPJF0PZUV86qdKx2r7d";
        public PurchaseServices(IUnitOfWork unitOfWork, IDataAccessHelper dataAccessHelper, IMapper mapper, IMessagePublisher<PurchaseDTO> messagePublisher, ILogger<PurchaseServices> logger)
        {
            _unitOfWork = unitOfWork;
            _dataAccessHelper = dataAccessHelper;
            _mapper = mapper;
            _messagePublisher = messagePublisher;
            _settings = RabbitMQConfigurations.DefaultSettings;
            _logger = logger;
        }

        public async Task<PurchaseDTO> GetPurchaseByIdAsync(int id)
        {
            var purchase = await _unitOfWork.Repository<Purchase>().GetByIdAsync(id);
            return _mapper.Map<PurchaseDTO>(purchase);
        }

        public async Task<IEnumerable<PurchaseDTO>> GetAllPurchasesAsync()
        {
            var purchases = await _unitOfWork.Repository<Purchase>().GetAllAsync();
            return _mapper.Map<IEnumerable<PurchaseDTO>>(purchases);
        }

        public async Task<IActionResult> AddPurchaseAsync(PurchaseDTO purchaseDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();

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

            try
            {
                // Publish the OrderCreated event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<PurchaseDTO>(purchase), _settings.Queues.PurchaseCreated);
                _logger.LogInformation($"PurchaseCreated event published successfully for the {_settings.Queues.PurchaseCreated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing PurchaseCreated event to RabbitMQ");
                throw;
            }

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

            try
            {
                // Publish the purchaseupdated event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<PurchaseDTO>(existingPurchase), _settings.Queues.PurchaseUpdated);
                _logger.LogInformation($"PurchaseUpdated event published successfully for the {_settings.Queues.PurchaseUpdated}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing PurchaseUpdated event to RabbitMQ");
                throw;
            }

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

            try
            {
                // Publish the PurchaseDeleted event message by using RabbitMQ
                await _messagePublisher.PublishAsync(_mapper.Map<PurchaseDTO>(purchase), _settings.Queues.PurchaseDeleted);
                _logger.LogInformation($"PurchaseDeleted event published successfully for the {_settings.Queues.PurchaseDeleted}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing PurchaseDeleted event to RabbitMQ");
                throw;
            }

            return new OkObjectResult(new { message = "Purchase deleted successfully." });
        }
    }
}
