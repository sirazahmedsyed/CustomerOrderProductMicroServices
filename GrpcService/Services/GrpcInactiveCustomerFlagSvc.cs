using Grpc.Core;
using GrpcService;
using GrpcService.Repository;

namespace GrpcService.Services
{
    public class GrpcInactiveCustomerFlagSvc : InactiveCustomerFlagService.InactiveCustomerFlagServiceBase
    {
        private readonly IInactiveFlagRepository _inactiveFlagRepository;

        public GrpcInactiveCustomerFlagSvc(IInactiveFlagRepository inactiveFlagRepository)
        {
            _inactiveFlagRepository = inactiveFlagRepository;
        }

        public override async Task<InactiveCustomerFlagResponse> GetInactiveCustomerFlag(InactiveCustomerFlagRequest req, ServerCallContext context)
        {
            var data = await _inactiveFlagRepository.GetInactiveCustomerFlagAsync(req);

            if (data != null)
            {
                context.Status = new Status(StatusCode.OK, "Inactive flag retrieved successfully");
                return await Task.FromResult(data);
            }
            else
            {
                context.Status = new Status(StatusCode.NotFound, "Inactive flag not found");
                return await Task.FromResult(new InactiveCustomerFlagResponse());
            }
        }
    }
}
