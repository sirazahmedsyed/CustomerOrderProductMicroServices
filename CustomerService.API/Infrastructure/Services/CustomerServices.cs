using AutoMapper;
using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Entities;
using CustomerService.API.Infrastructure.UnitOfWork;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.RedisCache;
using SharedRepository.Repositories;

namespace CustomerService.API.Infrastructure.Services
{
    public class CustomerServices : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly ILogger<CustomerServices> _logger;
        private readonly ICacheService _cacheService;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string ALL_CUSTOMERS_KEY = "customers:all";
        private const string CUSTOMER_KEY_PREFIX = "customer:";
        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";

        public CustomerServices(IUnitOfWork unitOfWork, IMapper mapper,IDataAccessHelper dataAccessHelper,
            ILogger<CustomerServices> logger,ICacheService cacheService, IRabbitMQHelper rabbitMQHelper,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dataAccessHelper = dataAccessHelper;
            _logger = logger;
            _cacheService = cacheService;
            _rabbitMQHelper = rabbitMQHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            var cachedCustomers = await _cacheService.GetAsync<IEnumerable<CustomerDTO>>(ALL_CUSTOMERS_KEY);
            if (cachedCustomers != null)
            {
                _logger.LogInformation("Retrieved customers from cache");
                return cachedCustomers;
            }

            var customers = await _unitOfWork.Repository<Customer>().GetAllAsync();

            await _cacheService.SetAsync(ALL_CUSTOMERS_KEY, _mapper.Map<IEnumerable<CustomerDTO>>(customers), TimeSpan.FromMinutes(5));

            return _mapper.Map<IEnumerable<CustomerDTO>>(customers);
        }

        public async Task<IActionResult> GetCustomerByIdAsync(Guid customerId)
        {
            var cacheKey = $"{CUSTOMER_KEY_PREFIX}{customerId}";

            var cachedCustomer = await _cacheService.GetAsync<CustomerDTO>(cacheKey);
            if (cachedCustomer != null)
            {
                _logger.LogInformation($"Retrieved customer {customerId} from cache");
                return new OkObjectResult(new { customerDto = cachedCustomer });
            }

            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
            if (customer == null)
            {
                return new BadRequestObjectResult(new { message = $"Customer with ID {customerId} not found." });
            }

            await _cacheService.SetAsync(cacheKey, _mapper.Map<CustomerDTO>(customer), TimeSpan.FromMinutes(30));

            return new OkObjectResult(new { customerDto = _mapper.Map<CustomerDTO>(customer) });
        }

        public async Task<IActionResult> AddCustomerAsync(CustomerDTO customerDto)
        {
            var existingCustomer = await _dataAccessHelper.CheckEmailExistsAsync(customerDto.Email);

            if (existingCustomer.EmailExists)
            {
                return new BadRequestObjectResult(new { message = $"Duplicate customer not allowed for this {customerDto.Email}." });
            }

            var customerEntity = _mapper.Map<Customer>(customerDto);
            customerEntity.CustomerId = Guid.NewGuid();
            await _unitOfWork.Repository<Customer>().AddAsync(customerEntity);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(ALL_CUSTOMERS_KEY);
            
            await SendAuditMessage(1, customerEntity.CustomerId, "Created");

            return new OkObjectResult(new
            {
                message = "Customer added successfully.",
                customer = _mapper.Map<CustomerDTO>(customerEntity)
            });
        }

        public async Task<IActionResult> UpdateCustomerAsync(CustomerDTO customerDto)
        {
            using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var existingCustomer = await connection.QueryAsync<Customer>($"SELECT * FROM customers WHERE customer_id = '{customerDto.CustomerId}'");

            if (existingCustomer.Any())
            {
                var customerToUpdate = existingCustomer.First();
                _mapper.Map(customerDto, customerToUpdate);
                _unitOfWork.Repository<Customer>().Update(customerToUpdate);
                await _unitOfWork.CompleteAsync();

                await _cacheService.RemoveAsync($"{CUSTOMER_KEY_PREFIX}{customerDto.CustomerId}");
                await _cacheService.RemoveAsync(ALL_CUSTOMERS_KEY);

                await SendAuditMessage(2, customerDto.CustomerId, "Updated");

                return new OkObjectResult(new
                {
                    message = "Customer updated successfully.",
                    customer = _mapper.Map<CustomerDTO>(customerToUpdate)
                });
            }

            return new BadRequestObjectResult(new { message = $"Customer is not found with this {customerDto.CustomerId} CustomerId." });
        }

        public async Task<IActionResult> DeleteCustomerAsync(Guid customerId)
        {
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
            if (customer != null)
            {
                _unitOfWork.Repository<Customer>().Remove(customer);
                await _unitOfWork.CompleteAsync();

                await _cacheService.RemoveAsync($"{CUSTOMER_KEY_PREFIX}{customerId}");
                await _cacheService.RemoveAsync(ALL_CUSTOMERS_KEY);

                await SendAuditMessage(3, customerId, "Deleted");

                return new OkObjectResult(new { message = "Customer deleted successfully." });
            }

            return new BadRequestObjectResult(new { message = "Customer not found." });
        }

        private async Task SendAuditMessage(int operationType, Guid customerId, string action)
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            var auditMessageDto = new AuditMessageDto
            {
                OprtnTyp = operationType,
                UsrNm = username,
                UsrNo = 1,
                LogDsc = new List<string> { $"{action} By {username} {DateTime.UtcNow:ddd MMM dd HH:mm:ss 'UTC' yyyy}" },
                LogTyp = 1,
                LogDate = DateTime.UtcNow,
                ScreenName = "CustomersController",
                ObjectName = "customer",
                ScreenPk = customerId
            };
            await _rabbitMQHelper.AuditResAsync(auditMessageDto);
        }
    }
}