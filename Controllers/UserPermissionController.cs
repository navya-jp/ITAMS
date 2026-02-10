using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ITAMS.Services.RBAC;
using ITAMS.Domain.Entities.RBAC;
using ITAMS.Models;
using System.Security.Claims;

namespace ITAMS.Controllers;

[ApiController]
[Route("api/rbac/users")]
[Authorize] // Require authentication for all endpoints
public class UserPermissionController : ControllerBase
{
    private readonly IPermissionResolver _permissionResolver;
    private readonly IRbacAuditService _auditService;
    private readonly ILogger<UserPermissionController> _logger;

    public UserPermissionController(
        IPermissionResolver permissionResolver,
        IRbacAuditService auditService,
        ILogger<UserPermissionController> logger)
    {
        _permissionResolver = permissionResolver;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get user's effective permissions summary
    /// </summary>
    [HttpGet("{userId}/permissions")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var permissionSummary = await _permissionResolver.GetUserPermissionSummaryAsync(userId);

            return Ok(new ApiResponse<UserPermissionSummary>
            {
                Success = true,
                Data = permissionSummary,
                Message = "User permissions retrieved successfully"
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while retrieving permissions for user {UserId}", userId);
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving user permissions",
                Error = ex.Message
            });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
