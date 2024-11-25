using GrpcService;
namespace GrpcService.Repository
{
    public interface IProductDetailsRepository
    {
        Task<ProductDetailsResponse> GetProductDetailsAsync(ProductDetailsRequest request);
    }
}
