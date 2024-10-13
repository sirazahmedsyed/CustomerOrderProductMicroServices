using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedRepository.Authorization;
using UserService.API.Infrastructure.DTOs;
using UserService.API.Infrastructure.Services;

namespace UserService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("GetUserById/{id:int}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"User with ID {id} not found");

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting user with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("CreateUser")]
        [Authorize(Policy = Permissions.AddUser)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user creation");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, message, createdUser) = await _userService.CreateUserAsync(userDto);

                if (!isSuccess)
                {
                    _logger.LogWarning(message);
                    if (message.Contains("does not exist"))
                    {
                        return NotFound(message); 
                    }
                    return Conflict(message); 
                }

                return Ok(createdUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("UpdateUserById")]
        public async Task<IActionResult> UpdateUserById([FromBody] UpdateUserDTO userDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user update.");
                return BadRequest(ModelState);
            }

            try
            {
                var (isSuccess, message, updatedUser) = await _userService.UpdateUserAsync(userDto);

                if (!isSuccess)
                {
                    _logger.LogWarning(message); 
                    return NotFound(message);    
                }

                _logger.LogInformation("User with ID {userDto.UserNo} updated successfully", userDto.UserNo);
                return Ok(updatedUser); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating user with ID {userDto.UserNo}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("DeleteUserById/{id:int}")]
        public async Task<IActionResult> DeleteUserById(int id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order creation");
                return BadRequest(ModelState);
            }
            try
            {
                _logger.LogInformation("Deleting user with ID {id}", id);
                var deletedUser = await _userService.DeleteUserAsync(id);
                if (!deletedUser)
                {
                    _logger.LogWarning("User with ID {id} not found", id);
                    return NotFound($"User with ID {id} not found.");
                }
                return Ok(deletedUser);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
