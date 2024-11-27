using Grpc.Core;
using Grpc.Net.Client;
using GrpcService;

namespace GrpcClient
{
    public class CustomerClient
    {
        private readonly GrpcChannel _channel;
        private readonly GrpcService.CustomerService.CustomerServiceClient _client;

        public CustomerClient()
        {
            _channel = GrpcChannel.ForAddress("https://localhost:7016");
            _client = new GrpcService.CustomerService.CustomerServiceClient(_channel);
        }

        public async Task<EmailResponse> CheckEmailExistsAsync(string email)
        {
            try
            {
                var request = new EmailRequest { Email = email };
                var response = await _client.CheckEmailExistsAsync(request);
                return response;
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Error calling gRPC service: {ex.Status.Detail}");
                throw;
            }
        }
    }
}
