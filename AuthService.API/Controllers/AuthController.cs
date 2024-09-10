using Microsoft.AspNetCore.Mvc;
using AuthMicroservice.Infrastructure.Services;
using AuthMicroservice.Infrastructure.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AuthMicroservice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
       
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.AuthenticateAsync(loginDto);
            if (result.Succeeded)
            {
                return Ok(new { Token = result.Token });
            }
            return Unauthorized();
        }

		[HttpPost]
		[Route("")]
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

        [Authorize]
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

    }
}