using Grpc.Core;
using GrpcService;
using GrpcService.Repository;
using Npgsql;

namespace GrpcService.Services
{
    public class GrpcProductDetailsService : ProductDetailsService.ProductDetailsServiceBase
    {
        private readonly IProductDetailsRepository _productDetailsRepository;

        public GrpcProductDetailsService(IProductDetailsRepository productDetailsRepository)
        {
            _productDetailsRepository = productDetailsRepository;
        }

        public override async Task<ProductDetailsResponse> GetProductDetails(ProductDetailsRequest req, ServerCallContext context)
        {
            var data = await _productDetailsRepository.GetProductDetailsAsync(req);

            if (data != null)
            {
                context.Status = new Status(StatusCode.OK, "Product details retrieved successfully");
                return await Task.FromResult(data);
            }
            else
            {
                context.Status = new Status(StatusCode.NotFound, "Product details not found");
                return await Task.FromResult(new ProductDetailsResponse());
            }
        }
    }
}


