using AutoMapper;
using ProductService.API.Infrastructure.DTOs;
using ProductService.API.Infrastructure.Entities;
using ProductService.API.Infrastructure.Profiles;

namespace ProductService.Tests.Infrastructure.Profiles
{
    public class MappingProfileTests
    {
        [Fact]
        public void MappingProfile_ValidConfiguration()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<MappingProfile>();
            });

            // Act & Assert
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void Map_ProductToProductDTO_ShouldMapAllProperties()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<MappingProfile>();
            });
            var mapper = config.CreateMapper();

            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00M
            };

            // Act
            var productDto = mapper.Map<ProductDTO>(product);

            // Assert
            Assert.Equal(product.ProductId, productDto.ProductId);
            Assert.Equal(product.Name, productDto.Name);
            Assert.Equal(product.Description, productDto.Description);
            Assert.Equal(product.Price, productDto.Price);
        }

        [Fact]
        public void Map_ProductDTOToProduct_ShouldMapAllProperties()
        {
            // Arrange
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<MappingProfile>();
            });
            var mapper = config.CreateMapper();

            var productDto = new ProductDTO
            {
                ProductId = 1,
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00M
            };

            // Act
            var product = mapper.Map<Product>(productDto);

            // Assert
            Assert.Equal(productDto.ProductId, product.ProductId);
            Assert.Equal(productDto.Name, product.Name);
            Assert.Equal(productDto.Description, product.Description);
            Assert.Equal(productDto.Price, product.Price);
        }
    }
}
