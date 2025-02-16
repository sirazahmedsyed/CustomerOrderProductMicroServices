using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using RabbitMQHelper.Infrastructure.DTOs;
using RabbitMQHelper.Infrastructure.Helpers;
using SharedRepository.RedisCache;
using SharedRepository.Repositories;
using UserGroupService.API.Infrastructure.DTOs;
using UserGroupService.API.Infrastructure.Entities;
using UserGroupService.API.Infrastructure.UnitOfWork;

namespace UserGroupService.API.Infrastructure.Services
{
    public class UserGroupServices : IUserGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly ILogger<UserGroupServices> _logger;
        private readonly ICacheService _cacheService;
        private readonly IRabbitMQHelper _rabbitMQHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const string ALL_USERGROUPS_KEY = "usergroups:all";
        private const string USERGROUP_KEY_PREFIX = "usergroup:";
        private readonly string dbconnection = "Host=dpg-cuk9b12j1k6c73d5dg20-a.oregon-postgres.render.com;Database=order_management_db_284m;Username=netconsumer;Password=6j9xg3A37zfiU5iRMLqdJmt6YPN46wLZ";

        public UserGroupServices(IUnitOfWork unitOfWork, IMapper mapper, IDataAccessHelper dataAccessHelper,
            ILogger<UserGroupServices> logger, ICacheService cacheService, IRabbitMQHelper rabbitMQHelper,
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

        public async Task<IEnumerable<UserGroupDTO>> GetAllUserGroupsAsync()
        {
            var cachedUserGroups = await _cacheService.GetAsync<IEnumerable<UserGroupDTO>>(ALL_USERGROUPS_KEY);
            if (cachedUserGroups != null)
            {
                _logger.LogInformation("Retrieved user groups from cache");
                return cachedUserGroups;
            }

            var userGroups = await _unitOfWork.Repository<UserGroup>().GetAllAsync();

            await _cacheService.SetAsync(ALL_USERGROUPS_KEY, _mapper.Map<IEnumerable<UserGroupDTO>>(userGroups), TimeSpan.FromMinutes(5));

            return _mapper.Map<IEnumerable<UserGroupDTO>>(userGroups);
        }

        public async Task<IActionResult> GetUserGroupByIdAsync(int id)
        {
            var cacheKey = $"{USERGROUP_KEY_PREFIX}{id}";

            var cachedUserGroup = await _cacheService.GetAsync<UserGroupDTO>(cacheKey);
            if (cachedUserGroup != null)
            {
                _logger.LogInformation($"Retrieved user group {id} from cache");
                return new OkObjectResult(new { usergroup = cachedUserGroup });
            }

            var userGroup = await _unitOfWork.Repository<UserGroup>().GetByIdAsync(id);
            if (userGroup == null)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {id} not found." });
            }

            await _cacheService.SetAsync(cacheKey, _mapper.Map<UserGroupDTO>(userGroup), TimeSpan.FromMinutes(30));

            return new OkObjectResult(new { usergroup = _mapper.Map<UserGroupDTO>(userGroup) });
        }

        public async Task<IActionResult> CreateUserGroupAsync(CreateUserGroupDTO userGroupDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var existingUserGroup = await connection.QuerySingleOrDefaultAsync<int>(
                $"select user_group_no from user_groups where user_group_local_name = '{userGroupDto.UserGroupLocalName}' and user_group_foreign_name = '{userGroupDto.UserGroupForeignName}'");

            if (existingUserGroup != 0)
            {
                return new BadRequestObjectResult(new { message = "User group with similar data already exists." });
            }

            var userGroup = _mapper.Map<UserGroup>(userGroupDto);
            await _unitOfWork.Repository<UserGroup>().AddAsync(userGroup);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync(ALL_USERGROUPS_KEY);

            await SendAuditMessage(1, userGroup.UserGroupNo, "Created");

            return new OkObjectResult(new
            {
                message = "UserGroup created successfully.",
                usergroup = _mapper.Map<CreateUserGroupDTO>(userGroup)
            });
        }

        public async Task<IActionResult> UpdateUserGroupAsync(UpdateUserGroupDTO userGroupDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();

            var inactiveFlag = await _dataAccessHelper.GetInactiveFlagFromGrpcAsync(userGroupDto.UserGroupNo);

            if (!inactiveFlag)
            {
                return new BadRequestObjectResult(new { message = "User group is not active, cannot update." });
            }

            var existingUserGroup = await connection.QuerySingleOrDefaultAsync<UserGroup>($"select * from user_groups where user_group_no = {userGroupDto.UserGroupNo}");
            if (existingUserGroup == null)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {userGroupDto.UserGroupNo} not found." });
            }

            _mapper.Map(userGroupDto, existingUserGroup);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{USERGROUP_KEY_PREFIX}{userGroupDto.UserGroupNo}");
            await _cacheService.RemoveAsync(ALL_USERGROUPS_KEY);

            await SendAuditMessage(2, userGroupDto.UserGroupNo, "Updated");

            return new OkObjectResult(new
            {
                message = "UserGroup updated successfully.",
                usergroup = _mapper.Map<UserGroupDTO>(existingUserGroup)
            });
        }

        public async Task<IActionResult> DeleteUserGroupAsync(int id)
        {
            var userGroup = await _unitOfWork.Repository<UserGroup>().GetByIdAsync(id);
            if (userGroup == null)
            {
                return new BadRequestObjectResult(new { message = $"UserGroup with ID {id} not found." });
            }

            _unitOfWork.Repository<UserGroup>().Remove(userGroup);
            await _unitOfWork.CompleteAsync();

            await _cacheService.RemoveAsync($"{USERGROUP_KEY_PREFIX}{id}");
            await _cacheService.RemoveAsync(ALL_USERGROUPS_KEY);

            await SendAuditMessage(3, id, "Deleted");

            return new OkObjectResult(new { message = "UserGroup deleted successfully." });
        }

        private async Task SendAuditMessage(int operationType, int userGroupNo, string action)
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
                ScreenPk = new Guid(BitConverter.GetBytes(userGroupNo).Concat(new byte[12]).ToArray())
            };
            await _rabbitMQHelper.AuditResAsync(auditMessageDto);
        }
    }
}
