using Grpc.Core;
using GrpcService;
using GrpcService.Repository;

namespace GrpcService.Services
{
    public class GrpcCustomerService : CustomerService.CustomerServiceBase
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<GrpcCustomerService> _logger;

        public GrpcCustomerService(ICustomerRepository customerRepository, ILogger<GrpcCustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public override async Task<EmailResponse> CheckEmailExists(EmailRequest req, ServerCallContext context)
        {
            try
            {
                var emailExists = await _customerRepository.CheckEmailExistsAsync(req);

                context.Status = new Status(StatusCode.OK, "Email check completed");
                return await Task.FromResult(emailExists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking email");
                context.Status = new Status(StatusCode.Internal, "Internal server error");
                return await Task.FromResult(new EmailResponse ());
            }
        }
    }
}


