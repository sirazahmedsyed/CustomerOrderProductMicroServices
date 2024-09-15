using Microsoft.AspNetCore.Mvc;
using AuthService.API.Infrastructure.Services;
using AuthService.API.Infrastructure.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AuthService.API.Infrastructure.Entities;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.AuthenticateAsync(loginDto);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);
                    return Ok(new { Token = result.Token });
                }

                _logger.LogWarning("Failed login attempt for user {Username}", loginDto.Username);
                return Unauthorized(new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Unauthorized",
                    Details = "Invalid credentials"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for user {Username}", loginDto.Username);
                return ErrorResponse(StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred during login.");
            }
        }

        [HttpPost]
        [Route("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            var result = await _authService.CreateUserAsync(createUserDto);
            if (result.Succeeded)
            {
                return Ok(new { Message = "User created successfully", UserId = result.UserId });
            }
            return BadRequest(result.Errors);
        }

        // [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            var result = await _authService.UpdateUserAsync(updateUserDto);
            if (result.Succeeded)
            {
                return Ok(new { Message = "User updated successfully" });
            }
            return BadRequest(result.Errors);
        }

        //[Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _authService.DeleteUserAsync(id);
            if (result.Succeeded)
            {
                return Ok(new { Message = "User deleted successfully" });
            }
            return BadRequest(result.Errors);
        }

        // [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new { Message = "Logged out successfully" });
        }

        private IActionResult ErrorResponse(int statusCode, string message, string details)
        {
            return StatusCode(statusCode, new ErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Details = details
            });
        }

    }
}