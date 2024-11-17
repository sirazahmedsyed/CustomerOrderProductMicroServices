using AutoMapper;
using UserGroupService.API.Infrastructure.Services;
using UserGroupService.API.Infrastructure.DTOs;
using UserGroupService.API.Infrastructure.Entities;
using UserGroupService.API.Infrastructure.UnitOfWork;
using Npgsql;
using System.Data.Common;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Data;
using Dapper;
using SharedRepository.Repositories;

namespace UserGroupService.API.Infrastructure.Services
{
    public class UserGroupServices : IUserGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDataAccessHelper _dataAccessHelper;
        private readonly string dbconnection = "Host=dpg-csl1qfrv2p9s73ae0iag-a.oregon-postgres.render.com;Database=inventorymanagement_h8uy;Username=netconsumer;Password=UBmEj8MjJqg4zlimlXovbyt0bBDcrmiF";

        public UserGroupServices(IUnitOfWork unitOfWork, IMapper mapper, IDataAccessHelper dataAccessHelper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dataAccessHelper = dataAccessHelper;
        }

        public async Task<IEnumerable<UserGroupDTO>> GetAllUserGroupsAsync()
        {
            var userGroups = await _unitOfWork.Repository<UserGroup>().GetAllAsync();
            return _mapper.Map<IEnumerable<UserGroupDTO>>(userGroups);
        }

        public async Task<UserGroupDTO> GetUserGroupByIdAsync(int id)
        {
            var userGroup = await _unitOfWork.Repository<UserGroup>().GetByIdAsync(id);
            return _mapper.Map<UserGroupDTO>(userGroup);
        }

        public async Task<CreateUserGroupDTO> CreateUserGroupAsync(CreateUserGroupDTO userGroupDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            var existingUserGroup = await connection.QuerySingleOrDefaultAsync<int>(
    $"SELECT user_group_no FROM user_groups WHERE user_group_local_name = '{userGroupDto.UserGroupLocalName}' AND user_group_foreign_name = '{userGroupDto.UserGroupForeignName}'");

            if (existingUserGroup != 0)
                {
                    Console.WriteLine($"User group with similar data already exists.");
                    return null; 
                }

            var userGroup = _mapper.Map<UserGroup>(userGroupDto);
            await _unitOfWork.Repository<UserGroup>().AddAsync(userGroup);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<CreateUserGroupDTO>(userGroup);
        }

        public async Task<UserGroupDTO> UpdateUserGroupAsync(UpdateUserGroupDTO userGroupDto)
        {
            var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");

            // Get inactive_flag using gRPC client through DataAccessHelper
            var inactiveFlag = await _dataAccessHelper.GetInactiveFlagFromGrpcAsync(userGroupDto.UserGroupNo);
            Console.WriteLine($"User group IN ACTIVE VALUE { inactiveFlag}");

            if (!inactiveFlag)
            {
                Console.WriteLine("User group is not active, cannot update.");
                return null;
            }
                
            var existingUserGroup = await connection.QuerySingleOrDefaultAsync<UserGroup>($"SELECT * FROM user_groups WHERE user_group_no = {userGroupDto.UserGroupNo}");
            if (existingUserGroup == null)
                return null;

            _mapper.Map(userGroupDto, existingUserGroup);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<UserGroupDTO>(existingUserGroup);
        }

        public async Task<bool> DeleteUserGroupAsync(int id)
        {
            var userGroup = await _unitOfWork.Repository<UserGroup>().GetByIdAsync(id);
            if (userGroup == null)
                return false;

            _unitOfWork.Repository<UserGroup>().Remove(userGroup);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}
