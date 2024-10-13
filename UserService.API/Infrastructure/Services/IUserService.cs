using UserService.API.Infrastructure.DTOs;

namespace UserService.API.Infrastructure.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(int userNo);
        Task<(bool isSuccess, string message, UserDTO user)> CreateUserAsync(CreateUserDTO userDto);
        Task<(bool isSuccess, string message, UserDTO user)> UpdateUserAsync(UpdateUserDTO userDto);
        Task<bool> DeleteUserAsync(int userNo);
    }
}
