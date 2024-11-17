namespace GrpcService.Repository
{
    public interface IInactiveFlagRepository
    {
        Task<InactiveFlagResponse> GetInactiveFlagAsync(InactiveFlagRequest request);
    }
}
