using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using UserService.API.Infrastructure.DTOs;
using UserService.API.Infrastructure.Entities;
using UserService.API.Infrastructure.UnitOfWork;
using SharedRepository.RedisCache;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;

namespace UserService.API.Infrastructure.Services
{
    public class UserServices : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserServices> _logger;
        private readonly ICacheService _cacheService;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string ALL_USERS_KEY = "users:all";
        private const string USER_KEY_PREFIX = "user:";
        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";

        public UserServices(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserServices> logger, 
            ICacheService cacheService, IRabbitMQHelper rabbitMQHelper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
            _rabbitMQHelper = rabbitMQHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
        {
            var cachedUsers = await _cacheService.GetAsync<IEnumerable<UserDTO>>(ALL_USERS_KEY);
            if (cachedUsers != null)
            {
                _logger.LogInformation("Retrieved users from cache");
                return cachedUsers;
            }

            var users = await _unitOfWork.Repository<User>().GetAllAsync();

            await _cacheService.SetAsync(ALL_USERS_KEY, _mapper.Map<IEnumerable<UserDTO>>(users), TimeSpan.FromMinutes(5));

            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> GetUserByIdAsync(int userNo)
        {
            var cacheKey = $"{USER_KEY_PREFIX}{userNo}";

            var cachedUser = await _cacheService.GetAsync<UserDTO>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation($"Retrieved user {userNo} from cache");
                return cachedUser;
            }

            var user = await _unitOfWork.Repository<User>().GetByIdAsync(userNo);
            if (user == null) return null;

            await _cacheService.SetAsync(cacheKey, _mapper.Map<UserDTO>(user), TimeSpan.FromMinutes(30));

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<IActionResult> CreateUserAsync(CreateUserDTO userDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var userExists = await connection.QuerySingleOrDefaultAsync<int>($"select count(*) from users where email = '{userDto.Email}'");

            if (userExists > 0)
            {
                return new BadRequestObjectResult(new { message = $"User with email {userDto.Email} already exists." });
            }

            var userGroupNoExists = await connection.QuerySingleOrDefaultAsync<int>($"select user_group_no from user_groups where user_group_no = '{userDto.UserGroupNo}'");

            if (userGroupNoExists == 0)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {userDto.UserGroupNo} does not exist." });
            }

            var user = _mapper.Map<User>(userDto);
            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(ALL_USERS_KEY);

            await SendAuditMessage(1, user.UserNo, "Created");

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

            var existingUser = await connection.QuerySingleOrDefaultAsync<User>($"select * from users where user_no = '{userDto.UserNo}'");

            if (existingUser == null)
            {
                return new BadRequestObjectResult(new { message = $"User with UserNO {userDto.UserNo} does not exist." });
            }

            var userGroupNoExists = await connection.QuerySingleOrDefaultAsync<int>($"select user_group_no from user_groups where user_group_no = '{userDto.UserGroupNo}'");

            if (userGroupNoExists == 0)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {userDto.UserGroupNo} does not exist." });
            }

            _mapper.Map(userDto, existingUser);
            _unitOfWork.Repository<User>().Update(existingUser);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{USER_KEY_PREFIX}{userDto.UserNo}");
            await _cacheService.RemoveAsync(ALL_USERS_KEY);

            await SendAuditMessage(2, userDto.UserNo, "Updated");

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

            await _cacheService.RemoveAsync($"{USER_KEY_PREFIX}{userNo}");
            await _cacheService.RemoveAsync(ALL_USERS_KEY);

            await SendAuditMessage(3, userNo, "Delete");
            return new OkObjectResult(new { message = "User deleted successfully." });
        }

        private async Task SendAuditMessage(int operationType, int userNo, string action)
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
                ScreenName = "UserGroupsController",
                ObjectName = "usergroup",
                ScreenPk = new Guid(BitConverter.GetBytes(userNo).Concat(new byte[12]).ToArray())
            };
            await _rabbitMQHelper.AuditResAsync(auditMessageDto);
        }
    }
}