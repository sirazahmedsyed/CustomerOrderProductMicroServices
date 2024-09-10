using AuthMicroservice.Infrastructure.DTOs;

namespace AuthMicroservice.Infrastructure.Services
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string Token)> AuthenticateAsync(LoginDto loginDto);

        Task<(bool Succeeded, string UserId, string[] Errors)> CreateUserAsync(CreateUserDto createUserDto);
        Task<(bool Succeeded, string[] Errors)> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<(bool Succeeded, string[] Errors)> DeleteUserAsync(string userId);
        Task LogoutAsync();

    }
}
