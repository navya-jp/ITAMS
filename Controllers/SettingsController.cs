using Microsoft.AspNetCore.Mvc;
using ITAMS.Data;
using ITAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : BaseController
    {
        private readonly ITAMSDbContext _context;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(
            ITAMSDbContext context,
            ILogger<SettingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSettings()
        {
            try
            {
                // Log all claims for debugging
                _logger.LogInformation("User claims: {Claims}", 
                    string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
                
                // Check if user is Super Admin by role name from JWT claims
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Super Admin")
                {
                    _logger.LogWarning("Non-admin user attempted to access settings. Role: {Role}", roleClaim?.Value ?? "None");
                    return Forbid();
                }

                var settings = await _context.SystemSettings
                    .OrderBy(s => s.Category)
                    .ThenBy(s => s.SettingKey)
                    .ToListAsync();

                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system settings");
                return StatusCode(500, new { message = "Error retrieving system settings" });
            }
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetSettingsByCategory(string category)
        {
            try
            {
                // Check if user is Super Admin by role name from JWT claims
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Super Admin")
                {
                    return Forbid();
                }

                var settings = await _context.SystemSettings
                    .Where(s => s.Category == category)
                    .OrderBy(s => s.SettingKey)
                    .ToListAsync();

                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving settings for category {Category}", category);
                return StatusCode(500, new { message = "Error retrieving settings" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSetting(int id)
        {
            try
            {
                // Check if user is Super Admin by role name from JWT claims
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Super Admin")
                {
                    return Forbid();
                }

                var setting = await _context.SystemSettings.FindAsync(id);
                
                if (setting == null)
                {
                    return NotFound(new { message = "Setting not found" });
                }

                return Ok(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving setting {Id}", id);
                return StatusCode(500, new { message = "Error retrieving setting" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSetting(int id, [FromBody] UpdateSettingRequest request)
        {
            try
            {
                // Check if user is Super Admin by role name from JWT claims
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Super Admin")
                {
                    return Forbid();
                }

                var setting = await _context.SystemSettings.FindAsync(id);
                
                if (setting == null)
                {
                    return NotFound(new { message = "Setting not found" });
                }

                if (!setting.IsEditable)
                {
                    return BadRequest(new { message = "This setting cannot be modified" });
                }

                // Validate the value based on data type
                if (!ValidateSettingValue(setting.DataType, request.SettingValue))
                {
                    return BadRequest(new { message = $"Invalid value for {setting.DataType} type" });
                }

                // Update the setting
                setting.SettingValue = request.SettingValue;
                setting.UpdatedBy = GetCurrentUserId();
                setting.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Setting {SettingKey} updated to {SettingValue} by user {UserId}", 
                    setting.SettingKey, setting.SettingValue, GetCurrentUserId());

                return Ok(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating setting {Id}", id);
                return StatusCode(500, new { message = "Error updating setting" });
            }
        }

        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkUpdateSettings([FromBody] List<UpdateSettingRequest> requests)
        {
            try
            {
                // Check if user is Super Admin by role name from JWT claims
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                if (roleClaim == null || roleClaim.Value != "Super Admin")
                {
                    return Forbid();
                }

                var updatedSettings = new List<SystemSetting>();

                foreach (var request in requests)
                {
                    var setting = await _context.SystemSettings.FindAsync(request.Id);
                    
                    if (setting == null || !setting.IsEditable)
                    {
                        continue;
                    }

                    if (!ValidateSettingValue(setting.DataType, request.SettingValue))
                    {
                        continue;
                    }

                    setting.SettingValue = request.SettingValue;
                    setting.UpdatedBy = GetCurrentUserId();
                    setting.UpdatedAt = DateTime.UtcNow;
                    
                    updatedSettings.Add(setting);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("{Count} settings updated by user {UserId}", 
                    updatedSettings.Count, GetCurrentUserId());

                return Ok(new { 
                    message = $"{updatedSettings.Count} settings updated successfully",
                    updatedSettings 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating settings");
                return StatusCode(500, new { message = "Error updating settings" });
            }
        }

        private bool ValidateSettingValue(string dataType, string value)
        {
            return dataType switch
            {
                "Integer" => int.TryParse(value, out _),
                "Boolean" => bool.TryParse(value, out _),
                "Decimal" => decimal.TryParse(value, out _),
                "String" => !string.IsNullOrWhiteSpace(value),
                _ => false
            };
        }
    }

    public class UpdateSettingRequest
    {
        public int Id { get; set; }
        public string SettingValue { get; set; } = string.Empty;
    }
}
