using AutoMapper;
using Dapper;
using Npgsql;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.API.Infrastructure.Services
{
    public class ProductServices : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
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

        public async Task<(bool IsSuccess, ProductDTO Product, string Message)> AddProductAsync(ProductDTO productDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var existingProductByName = await connection.QuerySingleOrDefaultAsync<string>($"SELECT name FROM products WHERE name = '{productDto.Name}'");

            if (existingProductByName!=null)
            {
                return (false, null, "Duplicate product not allowed for product name.");
            }

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return (true, _mapper.Map<ProductDTO>(product), "Product created successfully");
        }

        public async Task<ProductDTO> UpdateProductAsync(ProductDTO productDto)
        {

            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var existingProduct = await connection.QuerySingleOrDefaultAsync<Product>($"SELECT * FROM products WHERE product_id = '{productDto.ProductId}'");

            //var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
                return null;

            _mapper.Map(productDto, existingProduct); 
            _unitOfWork.Repository<Product>().Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ProductDTO>(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return false;

            _unitOfWork.Repository<Product>().Remove(product);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
