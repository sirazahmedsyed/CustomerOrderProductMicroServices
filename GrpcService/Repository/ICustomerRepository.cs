namespace GrpcService.Repository
{
    public interface ICustomerRepository
    {
        Task<EmailResponse> CheckEmailExistsAsync(EmailRequest request);
    }
}
