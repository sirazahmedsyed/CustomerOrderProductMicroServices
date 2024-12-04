using Microsoft.AspNetCore.Mvc;
using UserGroupService.API.Infrastructure.DTOs;

namespace UserGroupService.API.Infrastructure.Services
{
    public interface IUserGroupService
    {
        Task<IEnumerable<UserGroupDTO>> GetAllUserGroupsAsync();
        Task<IActionResult> CreateUserGroupAsync(CreateUserGroupDTO userGroupDto);
        Task<IActionResult> GetUserGroupByIdAsync(int id);
        Task<IActionResult> UpdateUserGroupAsync(UpdateUserGroupDTO userGroupDto);
        Task<IActionResult> DeleteUserGroupAsync(int id);
    }
}
