using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using UserService.API.Infrastructure.DTOs;
using UserService.API.Infrastructure.Entities;
using UserService.API.Infrastructure.UnitOfWork;

namespace UserService.API.Infrastructure.Services
{
    public class UserServices : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-ctaj11q3esus739aqeb0-a.oregon-postgres.render.com;Database=inventorymanagement_m3a1;Username=netconsumer;Password=y5oyt0LjENzsldOuO4zZ3mB2WbeM2ohw";
        public UserServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();
            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> GetUserByIdAsync(int userNo)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userNo);
            return _mapper.Map<UserDTO>(user);
        }


        public async Task<IActionResult> CreateUserAsync(CreateUserDTO userDto)
        {
           await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();
            Console.WriteLine($"Connection opened: {connection}");

            var userExists = await connection.QuerySingleOrDefaultAsync<int>
                ($"select count(*) from users where email = '{userDto.Email}'");

            if (userExists > 0)
            {
                return new BadRequestObjectResult(new { message = $"User with email {userDto.Email} already exists." });
            }

            var userGroupNoExists = await connection.QuerySingleOrDefaultAsync<int>(
                $"select user_group_no from user_groups where user_group_no = '{userDto.UserGroupNo}'");

            if (userGroupNoExists == 0)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {userDto.UserGroupNo} does not exist." });
            }

            var user = _mapper.Map<User>(userDto);
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new
            {
                message = "User created successfully.",
                user = _mapper.Map<UserDTO>(user)
            });
        }

        public async Task<IActionResult> UpdateUserAsync(UpdateUserDTO userDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();
            Console.WriteLine($"Connection opened: {connection}");

            var existingUser = await connection.QuerySingleOrDefaultAsync<User>
                ($"select * from users where user_no = '{userDto.UserNo}'");

            if (existingUser == null)
            {
                return new BadRequestObjectResult(new { message = $"User with UserNO {userDto.UserNo} does not exist." });
            }

            var userGroupNoExists = await connection.QuerySingleOrDefaultAsync<int>(
            $"select user_group_no from user_groups where user_group_no = '{userDto.UserGroupNo}'");

            if (userGroupNoExists == 0)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {userDto.UserGroupNo} does not exist." });
            }

            _mapper.Map(userDto, existingUser);
            _unitOfWork.Repository<User>().Update(existingUser);
            await _unitOfWork.CompleteAsync();

            return new OkObjectResult(new
            {
                message = "User updated successfully.",
                user = _mapper.Map<UserDTO>(existingUser)
            });
        }

        public async Task<IActionResult> DeleteUserAsync(int userNo)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userNo);
            if (user == null)
            {
                return new BadRequestObjectResult(new { message = $"User with ID {userNo} not found." });
            }

            _unitOfWork.Repository<User>().Remove(user);
            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(new { message = "User deleted successfully." });
        }
    }
}
