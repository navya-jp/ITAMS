using ITAMS.Data;
using ITAMS.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public SessionCleanupService(
            IServiceProvider serviceProvider,
            ILogger<SessionCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredSessions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during session cleanup");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Session Cleanup Service stopped");
        }

        private async Task CleanupExpiredSessions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();

            var now = DateTimeHelper.Now;

            // Load timeout from DB
            var timeoutSetting = await context.SystemSettings
                .Where(s => s.SettingKey == "SessionTimeoutMinutes" && s.Category == "Security")
                .Select(s => s.SettingValue)
                .FirstOrDefaultAsync();

            int timeoutMinutes = 30;

            if (!string.IsNullOrEmpty(timeoutSetting) &&
                int.TryParse(timeoutSetting, out var parsed))
            {
                timeoutMinutes = parsed;
            }

            var cutoffTime = now.AddMinutes(-timeoutMinutes);

            // Get users whose session expired
            var expiredUsers = await context.Users
                .Where(u =>
                    !string.IsNullOrEmpty(u.ActiveSessionId) &&
                    u.LastActivityAt.HasValue &&
                    u.LastActivityAt <= cutoffTime)
                .ToListAsync();

            if (!expiredUsers.Any())
                return;

            foreach (var user in expiredUsers)
            {
                // Find ACTIVE audit record
                var activeAudit = await context.LoginAudits
                    .Where(a => a.UserId == user.Id && a.Status == "ACTIVE")
                    .OrderByDescending(a => a.LoginTime)
                    .FirstOrDefaultAsync();

                if (activeAudit != null)
                {
                    activeAudit.Status = "SESSION_TIMEOUT";
                    activeAudit.LogoutTime = now;
                }

                // Clear session
                user.ActiveSessionId = null;
                user.SessionStartedAt = null;

                _logger.LogInformation(
                    "Session timeout for user {Username} after {Timeout} minutes",
                    user.Username,
                    timeoutMinutes
                );
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired sessions", expiredUsers.Count);
        }
    }
}
