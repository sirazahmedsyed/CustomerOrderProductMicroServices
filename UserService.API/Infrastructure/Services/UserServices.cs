using AutoMapper;
using Dapper;
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
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";
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

        public async Task<(bool isSuccess, string message, UserDTO user)> CreateUserAsync(CreateUserDTO userDto)
        {
            using var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            // Check if a user with the same email already exists
            var userExists = await connection.QuerySingleOrDefaultAsync<int>(
                $"SELECT COUNT(*) FROM public.\"Users\" WHERE \"Email\" = '{userDto.Email}'");

            if (userExists > 0)
            {
                return (false, $"User with email {userDto.Email} already exists.", null);
            }

            // Check if the UserGroupNo exists
            var userGroupNoExists = await connection.QuerySingleOrDefaultAsync<int>(
                $"SELECT \"UserGroupNo\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userDto.UserGroupNo}'");

            if (userGroupNoExists == 0)
            {
                return (false, $"User group with ID {userDto.UserGroupNo} does not exist.", null);
            }

            var user = _mapper.Map<User>(userDto);
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.CompleteAsync();
            return (true, "User created successfully", _mapper.Map<UserDTO>(user));
        }
        public async Task<(bool isSuccess, string message, UserDTO user)> UpdateUserAsync(UpdateUserDTO userDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var existingUser = await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM public.\"Users\" WHERE \"UserNo\" = '{userDto.UserNo}'");

            if (existingUser == null)
            {
                return (false, $"User with UserNO {userDto.UserNo} does not exists.", null);
            }

            var userGroupNoExists = await connection.QuerySingleOrDefaultAsync<int>(
            $"SELECT \"UserGroupNo\" FROM public.\"UserGroups\" WHERE \"UserGroupNo\" = '{userDto.UserGroupNo}'");
          
            if (userGroupNoExists == 0)
            {
                return (false, $"User group with ID {userDto.UserGroupNo} does not exist.", null);
            }

            //var user = await _unitOfWork.Repository<User>().GetByIdAsync(userDto.UserNo);
            _mapper.Map(userDto, existingUser);
            await _unitOfWork.CompleteAsync();
            return (true, "User created successfully", _mapper.Map<UserDTO>(existingUser));
        }
        
        public async Task<bool> DeleteUserAsync(int userNo)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userNo);
            if (user == null) 
            return false;
            _unitOfWork.Repository<User>().Remove(user);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
