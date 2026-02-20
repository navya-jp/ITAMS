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
        private readonly TimeSpan _forcedLogoutThreshold = TimeSpan.FromMinutes(2); // If no heartbeat for 2 minutes, browser likely closed

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
                    await CleanupAbandonedSessions();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during session cleanup");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Session Cleanup Service stopped");
        }

        private async Task CleanupAbandonedSessions()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();

            var now = DateTimeHelper.Now;
            var forcedLogoutCutoff = now.AddMinutes(-2); // No heartbeat for 2 minutes = browser closed

            // Find users whose browser closed (no heartbeat for 2+ minutes)
            var abandonedUsers = await context.Users
                .Where(u =>
                    !string.IsNullOrEmpty(u.ActiveSessionId) &&
                    u.LastActivityAt.HasValue &&
                    u.LastActivityAt <= forcedLogoutCutoff)
                .ToListAsync();

            if (!abandonedUsers.Any())
                return;

            foreach (var user in abandonedUsers)
            {
                // Find ACTIVE audit record
                var activeAudit = await context.LoginAudits
                    .Where(a => a.UserId == user.Id && a.Status == "ACTIVE")
                    .OrderByDescending(a => a.LoginTime)
                    .FirstOrDefaultAsync();

                if (activeAudit != null)
                {
                    activeAudit.Status = "FORCED_LOGOUT";
                    activeAudit.LogoutTime = now;
                }

                // Clear session
                user.ActiveSessionId = null;
                user.SessionStartedAt = null;

                _logger.LogInformation(
                    "Forced logout for user {Username} - no heartbeat for 2+ minutes (browser closed)",
                    user.Username
                );
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} abandoned sessions", abandonedUsers.Count);
        }
    }
}
