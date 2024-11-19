using GrpcService;

namespace GrpcService.Repository
{
    public interface IInactiveFlagRepository
    {
        Task<InactiveFlagResponse> GetInactiveFlagAsync(InactiveFlagRequest request);
        Task<InactiveCustomerFlagResponse> GetInactiveCustomerFlagAsync(InactiveCustomerFlagRequest request);
    }
}
