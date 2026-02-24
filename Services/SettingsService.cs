using ITAMS.Data;
using ITAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services;

public interface ISettingsService
{
    Task<string> GetSettingAsync(string key, string defaultValue = "");
    Task<int> GetIntSettingAsync(string key, int defaultValue = 0);
    Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false);
    Task<SecuritySettings> GetSecuritySettingsAsync();
    Task<bool> IsMaintenanceModeAsync();
}

public class SettingsService : ISettingsService
{
    private readonly ITAMSDbContext _context;
    private readonly ILogger<SettingsService> _logger;
    private Dictionary<string, string>? _cachedSettings;
    private DateTime _cacheExpiry = DateTime.MinValue;
    private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);

    public SettingsService(ITAMSDbContext context, ILogger<SettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task LoadSettingsAsync()
    {
        if (_cachedSettings == null || DateTime.UtcNow > _cacheExpiry)
        {
            try
            {
                var settings = await _context.SystemSettings.ToListAsync();
                _cachedSettings = settings.ToDictionary(s => s.SettingKey, s => s.SettingValue);
                _cacheExpiry = DateTime.UtcNow.Add(_cacheLifetime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading system settings");
                _cachedSettings = new Dictionary<string, string>();
            }
        }
    }

    public async Task<string> GetSettingAsync(string key, string defaultValue = "")
    {
        await LoadSettingsAsync();
        return _cachedSettings!.TryGetValue(key, out var value) ? value : defaultValue;
    }

    public async Task<int> GetIntSettingAsync(string key, int defaultValue = 0)
    {
        var value = await GetSettingAsync(key, defaultValue.ToString());
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false)
    {
        var value = await GetSettingAsync(key, defaultValue.ToString());
        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<SecuritySettings> GetSecuritySettingsAsync()
    {
        return new SecuritySettings
        {
            MaxLoginAttempts = await GetIntSettingAsync("MaxLoginAttempts", 5),
            LockoutDurationMinutes = await GetIntSettingAsync("LockoutDurationMinutes", 30),
            SessionTimeoutMinutes = await GetIntSettingAsync("SessionTimeoutMinutes", 30),
            PasswordExpiryDays = await GetIntSettingAsync("PasswordExpiryDays", 90),
            RequirePasswordChange = await GetBoolSettingAsync("RequirePasswordChange", true)
        };
    }

    public async Task<bool> IsMaintenanceModeAsync()
    {
        return await GetBoolSettingAsync("MaintenanceMode", false);
    }
}

public class SecuritySettings
{
    public int MaxLoginAttempts { get; set; }
    public int LockoutDurationMinutes { get; set; }
    public int SessionTimeoutMinutes { get; set; }
    public int PasswordExpiryDays { get; set; }
    public bool RequirePasswordChange { get; set; }
}
