using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using PurchaseService.API.Infrastructure.DTOs;
using PurchaseService.API.Infrastructure.Entities;
using PurchaseService.API.Infrastructure.UnitOfWork;
using SharedRepository.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PurchaseService.API.Infrastructure.Services
{
    public class PurchaseServices : IPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-csl1qfrv2p9s73ae0iag-a.oregon-postgres.render.com;Database=inventorymanagement_h8uy;Username=netconsumer;Password=UBmEj8MjJqg4zlimlXovbyt0bBDcrmiF";
        public PurchaseServices(IUnitOfWork unitOfWork, IDataAccessHelper dataAccessHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _dataAccessHelper = dataAccessHelper;
            _mapper = mapper;
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

        public async Task<(bool IsSuccess, PurchaseDTO Purchase, string Message)> AddPurchaseAsync(PurchaseDTO purchaseDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();

            var purchaseExists = await _dataAccessHelper.ExistsAsync("purchases", "purchase_order_no", purchaseDto.PurchaseOrderNo);

            if (purchaseExists)
            {
                return (false, null, $"Duplicate purchase not allowed for this {purchaseDto.PurchaseOrderNo}.");
            }

            var productdetails = await _dataAccessHelper.GetProductDetailsAsync(purchaseDto.ProductId);
            if (productdetails.ProductId == default)
            {
                return (false, null, $"Product with ID {purchaseDto.ProductId} does not exist.");
            }

            var stockUpdated = await _dataAccessHelper.UpdateProductStockAsync(purchaseDto.ProductId, purchaseDto.Quantity);
            if (!stockUpdated)
            {
                return (false, null, $"Failed to update product stock.");
            }
            var purchase = _mapper.Map<Purchase>(purchaseDto);
            await _unitOfWork.Repository<Purchase>().AddAsync(purchase);
            await _unitOfWork.CompleteAsync();
            return (true, _mapper.Map<PurchaseDTO>(purchase), "Purchase created successfully");
        }


        public async Task<(bool IsSuccess, PurchaseDTO Purchase, string Message)> UpdatePurchaseAsync(PurchaseDTO updatedPurchaseDto)
        {
            var purchaseExists = await _dataAccessHelper.ExistsAsync("purchases", "purchase_order_no", updatedPurchaseDto.PurchaseOrderNo);
           
            if (!purchaseExists)
            {
                return (false, null, $"Purchase with ID {updatedPurchaseDto.PurchaseId} not found.");
            }

            var productDetails = await _dataAccessHelper.GetProductDetailsAsync(updatedPurchaseDto.ProductId);
            if (productDetails.ProductId == default)
            {
                return (false, null, $"Product with ID {updatedPurchaseDto.ProductId} does not exist.");
            }

            var existingPurchase = await _unitOfWork.Repository<Purchase>().GetByIdAsync(updatedPurchaseDto.PurchaseOrderNo);

            int quantityDifference = updatedPurchaseDto.Quantity - existingPurchase.Quantity;

            var stockUpdated = await _dataAccessHelper.UpdateProductStockAsync(updatedPurchaseDto.ProductId, quantityDifference);
            if (!stockUpdated)
            {
                return (false, null, "Failed to update product stock.");
            }
            
            _mapper.Map(updatedPurchaseDto, existingPurchase);
            _mapper.Map<Purchase>(updatedPurchaseDto);
            _unitOfWork.Repository<Purchase>().Update(existingPurchase);
            await _unitOfWork.CompleteAsync();

            return (true, _mapper.Map<PurchaseDTO>(existingPurchase), "Purchase updated successfully");
        }

        public async Task<(bool IsSuccess, string Message)> DeletePurchaseAsync(int purchaseId)
        {
            var existingPurchase = await _dataAccessHelper.ExistsAsync("purchases", "purchase_id", purchaseId);
            if (!existingPurchase)
            {
                return (false, $"Purchase with ID {purchaseId} not found.");
            }
            var purchase = await _unitOfWork.Repository<Purchase>().GetByIdAsync(purchaseId);

            var stockUpdated = await _dataAccessHelper.UpdateProductStockAsync(purchase.ProductId, -purchase.Quantity);
            if (!stockUpdated)
            {
                return (false, "Failed to update product stock.");
            }

            _unitOfWork.Repository<Purchase>().Remove(purchase);
            await _unitOfWork.CompleteAsync();
            return (true, "Purchase deleted successfully");
        }
    }
}
