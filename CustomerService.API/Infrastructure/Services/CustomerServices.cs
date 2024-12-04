using AutoMapper;
using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Entities;
using CustomerService.API.Infrastructure.UnitOfWork;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using SharedRepository.Repositories;

namespace CustomerService.API.Infrastructure.Services
{
    public class CustomerServices : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly string dbconnection = "Host=dpg-csl1qfrv2p9s73ae0iag-a.oregon-postgres.render.com;Database=inventorymanagement_h8uy;Username=netconsumer;Password=UBmEj8MjJqg4zlimlXovbyt0bBDcrmiF";
        public CustomerServices(IUnitOfWork unitOfWork, IMapper mapper, IDataAccessHelper dataAccessHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dataAccessHelper = dataAccessHelper;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Repository<Customer>().GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDTO>>(customers);
        }

        public async Task<IActionResult> GetCustomerByIdAsync(Guid customerId)
        {
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);

            if (customer == null)
            {
                return new BadRequestObjectResult(new { message = $"Customer with ID {customerId} not found." });
            }
            return new OkObjectResult(new
            {
                customerDto = _mapper.Map<CustomerDTO>(customer)
            });
        }

        public async Task<IActionResult> AddCustomerAsync(CustomerDTO customerDto)
        {
            var existingCustomer = await _dataAccessHelper.CheckEmailExistsAsync(customerDto.Email);
           
            if (!existingCustomer.EmailExists)
            {
                return new BadRequestObjectResult(new { message = $"Duplicate customer not allowed for this {customerDto.Email}." });
            }
            
            var customerEntity = _mapper.Map<Customer>(customerDto);
            customerEntity.CustomerId = Guid.NewGuid();
            await _unitOfWork.Repository<Customer>().AddAsync(customerEntity);
            await _unitOfWork.CompleteAsync();
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

            var existingCustomer = await connection.QueryAsync<Customer>(
            $"SELECT * FROM customers WHERE customer_id = '{customerDto.CustomerId}'");
            //var existingCustomer = await _unitOfWork.Repository<Customer>().FindAsync(c => c.CustomerId == customerDto.CustomerId);
            
            if (existingCustomer.Any())
            {
                var customerToUpdate = existingCustomer.First();
                _mapper.Map(customerDto, customerToUpdate);
                _unitOfWork.Repository<Customer>().Update(customerToUpdate);
                await _unitOfWork.CompleteAsync();

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
                return new OkObjectResult(new { message = "Customer deleted successfully." });
            }

            return new BadRequestObjectResult(new { message = "Customer not found." });
        }
    }
}
