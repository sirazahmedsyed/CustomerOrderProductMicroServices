using UserGroupService.API.Infrastructure.DTOs;

namespace UserGroupService.API.Infrastructure.Services
{
    public interface IUserGroupService
    {
        Task<CreateUserGroupDTO> CreateUserGroupAsync(CreateUserGroupDTO userGroupDto);
        Task<UserGroupDTO> GetUserGroupByIdAsync(int id);
        Task<IEnumerable<UserGroupDTO>> GetAllUserGroupsAsync();
        Task<UserGroupDTO> UpdateUserGroupAsync(UpdateUserGroupDTO userGroupDto);
        Task<bool> DeleteUserGroupAsync(int id);
    }
}
