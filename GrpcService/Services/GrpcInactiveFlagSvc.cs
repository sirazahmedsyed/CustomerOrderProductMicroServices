using Grpc.Core;
using GrpcService.Repository;

namespace GrpcService.Services
{
    public class GrpcInactiveFlagSvc : InactiveFlagService.InactiveFlagServiceBase
    {
        private readonly IInactiveFlagRepository _inactiveFlagRepository;

        public GrpcInactiveFlagSvc(IInactiveFlagRepository inactiveFlagRepository)
        {
            _inactiveFlagRepository = inactiveFlagRepository;
        }

        public override async Task<InactiveFlagResponse> GetInactiveFlag(InactiveFlagRequest req, ServerCallContext context)
        {
            var data = await _inactiveFlagRepository.GetInactiveFlagAsync(req);

            if (data != null)
            {
                context.Status = new Status(StatusCode.OK, "Inactive flag retrieved successfully");
                return await Task.FromResult(data);
            }
            else
            {
                context.Status = new Status(StatusCode.NotFound, "Inactive flag not found");
                return await Task.FromResult(new InactiveFlagResponse());
            }
        }
    }
}
