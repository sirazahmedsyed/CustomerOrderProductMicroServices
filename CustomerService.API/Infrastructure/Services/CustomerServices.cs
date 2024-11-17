using AutoMapper;
using CustomerService.API.Infrastructure.DTOs;
using CustomerService.API.Infrastructure.Entities;
using CustomerService.API.Infrastructure.UnitOfWork;
using Dapper;
using Npgsql;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CustomerService.API.Infrastructure.Services
{
    public class CustomerServices : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-csl1qfrv2p9s73ae0iag-a.oregon-postgres.render.com;Database=inventorymanagement_h8uy;Username=netconsumer;Password=UBmEj8MjJqg4zlimlXovbyt0bBDcrmiF";
        public CustomerServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllCustomersAsync()
        {
            var customers = await _unitOfWork.Repository<Customer>().GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerDTO>>(customers);
        }

            public async Task<CustomerDTO> GetCustomerByIdAsync(Guid customerId)
            {
                var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
                return customer == null ? null : _mapper.Map<CustomerDTO>(customer);
            }

        public async Task<(bool IsSuccess, Guid CustomerId, CustomerDTO Customer, string Message)> AddCustomerAsync(CustomerDTO customerDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var existingCustomer = await connection.QueryAsync<Customer>(
                $"SELECT Email FROM customers WHERE email = '{customerDto.Email}'");
            var emailExists = existingCustomer.Any(c => c.Email == customerDto.Email);
            if (existingCustomer.Any(c => c.Email != null))
            {
                return (false, Guid.Empty, null, "Customer with this email already exists.");
            }
            
            var customerEntity = _mapper.Map<Customer>(customerDto);
            customerEntity.CustomerId = Guid.NewGuid();
            await _unitOfWork.Repository<Customer>().AddAsync(customerEntity);
            await _unitOfWork.CompleteAsync();
            var addedCustomerDto = _mapper.Map<CustomerDTO>(customerEntity);
            return (true, customerEntity.CustomerId, addedCustomerDto, "Customer added successfully.");
        }

        public async Task<(bool IsSuccess, CustomerDTO Customer, string Message)> UpdateCustomerAsync(CustomerDTO customerDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
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

                var updatedCustomerDto = _mapper.Map<CustomerDTO>(customerToUpdate);
                return (true, updatedCustomerDto, "Customer updated successfully.");
            }
            return (false, null, "Customer not found.");
        }

        public async Task<(bool IsSuccess, string Message)> DeleteCustomerAsync(Guid customerId)
        {
            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
            if (customer != null)
            {
                _unitOfWork.Repository<Customer>().Remove(customer);
                await _unitOfWork.CompleteAsync();
                return (true, "Customer deleted successfully.");
            }

            return (false, "Customer not found.");
        }
    }
}
