using AutoMapper;
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
            // Check if the product already exists
            var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productDto.ProductId);

            if (existingProduct != null)
            {
                // Return false to indicate failure and a message that a duplicate exists
                return (false, null, "Duplicate product not allowed");
            }

            // Check for existing product by name (case-insensitive)
            var existingProductByName = await _unitOfWork.Repository<Product>()
                 .FindAsync(p => p.Name.ToLower() == productDto.Name.ToLower());

            if (existingProductByName.Any())
            {
                return (false, null, "Duplicate product not allowed for product name.");
            }

            var product = _mapper.Map<Product>(productDto);
            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.CompleteAsync();

            // Return success along with the created product
            return (true, _mapper.Map<ProductDTO>(product), "Product created successfully");
        }

        public async Task<ProductDTO> UpdateProductAsync(ProductDTO productDto)
        {
            var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(productDto.ProductId);
            if (existingProduct == null)
                return null;

            _mapper.Map(productDto, existingProduct); // Map updated fields from DTO to the entity
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
