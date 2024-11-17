using AutoMapper;
using Dapper;
using Npgsql;
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
        private readonly IProductHelper _productHelper;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";

        public PurchaseServices(IUnitOfWork unitOfWork, IProductHelper productHelper, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _productHelper = productHelper;
            _mapper = mapper;
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

            var existingPurchase = await connection.QuerySingleOrDefaultAsync<string>(
                $"SELECT purchase_order_no FROM purchases WHERE purchase_order_no = '{purchaseDto.PurchaseOrderNo}'");

            if (existingPurchase != null)
            {
                return (false, null, "Duplicate purchase order not allowed.");
            }

            //var product = await _unitOfWork.Repository<Product>().GetByIdAsync(purchaseDto.ProductId);
            //if (product == null)
            //{
            //    return (false, null, $"Product with ID {purchaseDto.ProductId} does not exist.");
            //}

            var productdetails = await _productHelper.GetProductDetailsAsync(purchaseDto.ProductId);
            if (productdetails.ProductId == default)
            {
                return (false, null, $"Product with ID {purchaseDto.ProductId} does not exist.");
            }

            var stockUpdated = await _productHelper.UpdateProductStockAsync(purchaseDto.ProductId, purchaseDto.Quantity);
            if (!stockUpdated)
            {
                return (false, null, $"Failed to update product stock.");
            }
            var purchase = _mapper.Map<Purchase>(purchaseDto);
            await _unitOfWork.Repository<Purchase>().AddAsync(purchase);
            await _unitOfWork.CompleteAsync();
            return (true, _mapper.Map<PurchaseDTO>(purchase), "Purchase created successfully");
        }
    }
}
