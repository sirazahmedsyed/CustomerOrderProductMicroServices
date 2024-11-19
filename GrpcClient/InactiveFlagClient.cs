using Grpc.Net.Client;
using GrpcService;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrpcClient
{
    public class InactiveFlagClient
    {
        private readonly GrpcChannel _channel;
        private readonly GrpcService.InactiveFlagService.InactiveFlagServiceClient _client;
        private readonly GrpcService.InactiveCustomerFlagService.InactiveCustomerFlagServiceClient _clientCustomer;

        public InactiveFlagClient()
        {
            _channel = GrpcChannel.ForAddress("https://localhost:7016");
            _client = new GrpcService.InactiveFlagService.InactiveFlagServiceClient(_channel);
            _clientCustomer = new GrpcService.InactiveCustomerFlagService.InactiveCustomerFlagServiceClient(_channel);
        }

        public async Task<bool> GetInactiveFlagAsync(int userGroupNo)
        {
            try
            {
                var request = new InactiveFlagRequest 
                { 
                    UserGroupNo = userGroupNo 
                };
                var response = await _client.GetInactiveFlagAsync(request);
                return response.InactiveFlag;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling gRPC service: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> GetInactiveCustomerFlagAsync(Guid CustomerId)
        {
            try
            {
                var request = new InactiveCustomerFlagRequest
                {
                    CustomerId = Google.Protobuf.ByteString.CopyFrom(CustomerId.ToByteArray())
                };
                var response = await _clientCustomer.GetInactiveCustomerFlagAsync(request);
                return response.InactiveFlag;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling gRPC service: {ex.Message}");
                throw;
            }
        }
    }
}
