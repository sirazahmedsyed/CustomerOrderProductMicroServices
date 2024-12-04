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
                var result = await _userService.CreateUserAsync(userDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

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
                var result = await _userService.UpdateUserAsync(userDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

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
                var result = await _userService.DeleteUserAsync(id);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
