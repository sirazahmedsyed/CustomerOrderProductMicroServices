using Microsoft.AspNetCore.Mvc;
using UserService.API.Infrastructure.DTOs;

namespace UserService.API.Infrastructure.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(int userNo);
        Task<IActionResult> CreateUserAsync(CreateUserDTO userDto);
        Task<IActionResult> UpdateUserAsync(UpdateUserDTO userDto);
        Task<IActionResult> DeleteUserAsync(int userNo);
    }
}
