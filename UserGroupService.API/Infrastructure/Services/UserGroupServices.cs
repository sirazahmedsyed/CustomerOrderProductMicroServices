using AutoMapper;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
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
        private readonly string dbconnection = "Host=dpg-ctuh03lds78s73fntmag-a.oregon-postgres.render.com;Database=order_management_db;Username=netconsumer;Password=wv5ZjPAcJY8ICgPJF0PZUV86qdKx2r7d";

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

        public async Task<IActionResult> GetUserGroupByIdAsync(int id)
        {
            var userGroup = await _unitOfWork.Repository<UserGroup>().GetByIdAsync(id);
            if (userGroup == null)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {id} not found." });
            }
            return new ObjectResult( new { usergroup=_mapper.Map<UserGroupDTO>(userGroup) });
        }

        public async Task<IActionResult> CreateUserGroupAsync(CreateUserGroupDTO userGroupDto)
        {
            await using var connection = new NpgsqlConnection(dbconnection);
            await connection.OpenAsync();
            Console.WriteLine($"connection opened : {connection}");

            var existingUserGroup = await connection.QuerySingleOrDefaultAsync<int>(
    $"select user_group_no from user_groups where user_group_local_name = '{userGroupDto.UserGroupLocalName}' and user_group_foreign_name = '{userGroupDto.UserGroupForeignName}'");

            if (existingUserGroup != 0)
                {
                    Console.WriteLine($"User group with similar data already exists.");
                    return new BadRequestObjectResult(new { message = "User group with similar data already exists." });
            }

            var userGroup = _mapper.Map<UserGroup>(userGroupDto);
            await _unitOfWork.Repository<UserGroup>().AddAsync(userGroup);
            await _unitOfWork.CompleteAsync();
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
            Console.WriteLine($"connection opened : {connection}");

            var inactiveFlag = await _dataAccessHelper.GetInactiveFlagFromGrpcAsync(userGroupDto.UserGroupNo);
            Console.WriteLine($"User group in active value { inactiveFlag}");

            if (!inactiveFlag)
            {
                Console.WriteLine("User group is not active, cannot update.");
                return new BadRequestObjectResult(new { message = "User group is not active, cannot update." });
            }
                
            var existingUserGroup = await connection.QuerySingleOrDefaultAsync<UserGroup>($"select * from user_groups where user_group_no = {userGroupDto.UserGroupNo}");
            if (existingUserGroup == null)
            {
                return new BadRequestObjectResult(new { message = $"User group with ID {userGroupDto.UserGroupNo} not found." });
            }
            _mapper.Map(userGroupDto, existingUserGroup);
            await _unitOfWork.CompleteAsync();
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
            return new ObjectResult(new 
            { 
                message = "UserGroup deleted successfully."
            });
        }
    }
}
