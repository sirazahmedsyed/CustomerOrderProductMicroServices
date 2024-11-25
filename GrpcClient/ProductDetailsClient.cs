using Grpc.Net.Client;
using GrpcService;

public class ProductDetailsClient
{
    private readonly GrpcChannel _channel;
    private readonly ProductDetailsService.ProductDetailsServiceClient _client;

    public ProductDetailsClient()
    {
        _channel = GrpcChannel.ForAddress("https://localhost:7016");
        _client = new ProductDetailsService.ProductDetailsServiceClient(_channel);
    }

    public async Task<ProductDetailsResponse> GetProductDetailsAsync(int productId)
    {
        try
        {
            var request = new ProductDetailsRequest
            {
                ProductId = productId
            };
            var response = await _client.GetProductDetailsAsync(request);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling gRPC service: {ex.Message}");
            throw;
        }
    }
}
