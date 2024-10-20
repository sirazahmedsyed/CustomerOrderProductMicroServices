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

namespace UserGroupService.API.Infrastructure.Services
{
    public class UserGroupServices : IUserGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly string dbconnection = "Host=dpg-crvsqllds78s738bvq40-a.oregon-postgres.render.com;Database=user_usergroupdatabase;Username=user_usergroupdatabase_user;Password=X01Sf7FT75kppHe46dnULUCpe52s69ag";

        public UserGroupServices(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
