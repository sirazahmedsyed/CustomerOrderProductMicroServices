using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedRepository.Authorization;
using UserGroupService.API.Infrastructure.DTOs;
using UserGroupService.API.Infrastructure.Services;
using Microsoft.Extensions.Logging;

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
                var userGroup = await _userGroupService.GetUserGroupByIdAsync(id);
                if (userGroup == null)
                {
                    _logger.LogWarning($"User group with ID {id} not found");
                    return NotFound($"User group with ID {id} not found");
                }
                return Ok(userGroup);
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
                var createdUserGroup = await _userGroupService.CreateUserGroupAsync(userGroupDto);
                if (createdUserGroup == null)
                {
                    return NotFound($"Duplicate UserGroup with ID {userGroupDto.UserGroupNo} not allowed");
                }
                return Ok(createdUserGroup);
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
                var updatedUserGroup = await _userGroupService.UpdateUserGroupAsync(userGroupDto);
                if (updatedUserGroup == null)
                {
                    _logger.LogWarning($"User group with ID {userGroupDto.UserGroupNo} not found");
                    return NotFound($"User group with ID {userGroupDto.UserGroupNo} not found.");
                }
                _logger.LogInformation($"User group with ID {userGroupDto.UserGroupNo} updated");
                return Ok(updatedUserGroup);
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
                _logger.LogInformation($"Deleting user group with ID {id}");
                var deletedUserGroup = await _userGroupService.DeleteUserGroupAsync(id);
                if (!deletedUserGroup)
                {
                    _logger.LogWarning($"User group with ID {id} not found");
                    return NotFound($"User group with ID {id} not found.");
                }
                return Ok(deletedUserGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting user group with id {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
