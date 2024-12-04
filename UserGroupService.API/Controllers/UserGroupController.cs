using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedRepository.Authorization;
using UserGroupService.API.Infrastructure.DTOs;
using UserGroupService.API.Infrastructure.Services;

namespace UserGroupService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserGroupController : ControllerBase
    {
        private readonly IUserGroupService _userGroupService;
        private readonly ILogger<UserGroupController> _logger;

        public UserGroupController(IUserGroupService userGroupService, ILogger<UserGroupController> logger)
        {
            _userGroupService = userGroupService;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetAllUserGroups")]
        public async Task<IActionResult> GetAllUserGroups()
        {
            try
            {
                var userGroups = await _userGroupService.GetAllUserGroupsAsync();
                return Ok(userGroups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all user groups");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("GetUserGroupById/{id:int}")]
        public async Task<IActionResult> GetUserGroupById(int id)
        {
            try
            {
                var result = await _userGroupService.GetUserGroupByIdAsync(id);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting user group with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Route("CreateUserGroup")]
        [Authorize(Policy = Permissions.AddUserGroup)]
        public async Task<IActionResult> CreateUserGroup([FromBody] CreateUserGroupDTO userGroupDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user group creation");
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _userGroupService.CreateUserGroupAsync(userGroupDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new user group");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut]
        [Route("UpdateUserGroupById")]
        public async Task<IActionResult> UpdateUserGroupById([FromBody] UpdateUserGroupDTO userGroupDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for user group update");
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _userGroupService.UpdateUserGroupAsync(userGroupDto);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating user group with id {userGroupDto.UserGroupNo}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete]
        [Route("DeleteUserGroupById/{id:int}")]
        public async Task<IActionResult> DeleteUserGroupById(int id)
        {
            try
            {
                var result = await _userGroupService.DeleteUserGroupAsync(id);

                if (result is BadRequestObjectResult badRequestResult)
                {
                    var errorMessage = badRequestResult.Value?.ToString();
                    _logger.LogWarning(errorMessage);
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user group with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

