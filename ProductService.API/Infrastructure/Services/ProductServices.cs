﻿using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.UnitOfWork;

namespace ProductService.API.Infrastructure.Services
{
    public class ProductServices : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-ctaj11q3esus739aqeb0-a.oregon-postgres.render.com;Database=inventorymanagement_m3a1;Username=netconsumer;Password=y5oyt0LjENzsldOuO4zZ3mB2WbeM2ohw";
        public ProductServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDTO>> GetAllProductsAsync()
        {
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task<IActionResult> AddProductAsync(ProductDTO productDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();
            Console.WriteLine($"connection opened : {connection}");

            var existingProductByName = await connection.QuerySingleOrDefaultAsync<string>($"select name from products where name = '{productDto.Name}'");

            if (existingProductByName != null)
            {
                return new BadRequestObjectResult(new { message = $"Duplicate product not allowed for this product {productDto.Name}" });
            }

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new
            {
                message = "Product created successfully",
                product = _mapper.Map<ProductDTO>(product)
            });
        }

        public async Task<IActionResult> UpdateProductAsync(ProductDTO productDto)
        {

            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var existingProduct = await connection.QuerySingleOrDefaultAsync<Product>($"SELECT * FROM products WHERE product_id = '{productDto.ProductId}'");

            //var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
            { 
                return new BadRequestObjectResult(new { message = $"Product is not availble for this {productDto.ProductId} productId" });
            }

            _mapper.Map(productDto, existingProduct); 
            _unitOfWork.Repository<Product>().Update(existingProduct);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new
            {
                message = "Product Updated successfully",
                product = _mapper.Map<ProductDTO>(existingProduct)
            });
        }
        public async Task<IActionResult> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return new BadRequestObjectResult(new { message = $"Product with ID {id} not found." });
            }

            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new { message = "Product deleted successfully." });
        }
    }
}
