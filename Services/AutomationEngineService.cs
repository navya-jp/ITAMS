using ITAMS.Data;
using ITAMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ITAMS.Services;

public class AutomationEngineService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AutomationEngineService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public AutomationEngineService(IServiceProvider services, ILogger<AutomationEngineService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Automation Engine started.");

        // Run once at startup after a short delay, then every 24h
        await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunChecksAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Automation Engine encountered an error.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task RunChecksAsync()
    {
        _logger.LogInformation("Automation Engine: running daily checks at {Time}", DateTime.UtcNow);

        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

        await CheckWarrantyExpiryAsync(context, email);
        await CheckLicenseExpiryAsync(context, email);
        await CheckContractExpiryAsync(context, email);
        await CheckRepairStuckAsync(context, email);
        await CheckComplianceAsync(context, email);
        await EscalateUnacknowledgedAlertsAsync(context, email);

        await context.SaveChangesAsync();
        _logger.LogInformation("Automation Engine: daily checks complete.");
    }

    // ─── WARRANTY EXPIRY ────────────────────────────────────────────────────────

    private async Task CheckWarrantyExpiryAsync(ITAMSDbContext context, IEmailService email)
    {
        var today = DateTime.Today;
        var assets = await context.Assets
            .Include(a => a.Project)
            .Include(a => a.Location)
            .Where(a => a.WarrantyEndDate != null && a.WarrantyEndDate > today)
            .ToListAsync();

        foreach (var asset in assets)
        {
            var days = (asset.WarrantyEndDate!.Value - today).Days;
            string? severity = days <= 7 ? "Critical" : days <= 30 ? "High" : null;
            if (severity == null) continue;

            var alertType = "WARRANTY_EXPIRY";
            var exists = await context.SystemAlerts.AnyAsync(a =>
                a.AlertType == alertType && a.AssetId == asset.Id &&
                !a.IsResolved && a.CreatedAt.Date == today);
            if (exists) continue;

            var title = $"Warranty expiring in {days} day(s): {asset.AssetTag}";
            var msg = BuildEmailBody(title,
                $"Asset <b>{asset.AssetTag}</b> ({asset.Make} {asset.Model}) warranty expires on <b>{asset.WarrantyEndDate:dd-MMM-yyyy}</b>.<br>" +
                $"Project: {asset.Project?.Name ?? "N/A"} | Location: {asset.Location?.Name ?? "N/A"}");

            await CreateAlertAndNotify(context, email, new SystemAlert
            {
                AlertType = alertType,
                Severity = severity,
                Title = title,
                Message = msg,
                AssetId = asset.Id,
                EntityType = "Asset",
                EntityIdentifier = asset.AssetTag
            }, msg, title);
        }
    }

    // ─── LICENSE EXPIRY ─────────────────────────────────────────────────────────

    private async Task CheckLicenseExpiryAsync(ITAMSDbContext context, IEmailService email)
    {
        var today = DateTime.Today;
        var licenses = await context.LicensingAssets
            .Where(l => l.ValidityEndDate > today)
            .ToListAsync();

        foreach (var lic in licenses)
        {
            var days = (lic.ValidityEndDate - today).Days;
            string? severity = days <= 7 ? "Critical" : days <= 30 ? "High" : null;
            if (severity == null) continue;

            var alertType = "LICENSE_EXPIRY";
            var exists = await context.SystemAlerts.AnyAsync(a =>
                a.AlertType == alertType && a.LicensingAssetId == lic.Id &&
                !a.IsResolved && a.CreatedAt.Date == today);
            if (exists) continue;

            var title = $"License expiring in {days} day(s): {lic.LicenseName}";
            var msg = BuildEmailBody(title,
                $"License <b>{lic.LicenseName}</b> (v{lic.Version}) expires on <b>{lic.ValidityEndDate:dd-MMM-yyyy}</b>.<br>" +
                $"License Key: {lic.LicenseKey ?? "N/A"}");

            await CreateAlertAndNotify(context, email, new SystemAlert
            {
                AlertType = alertType,
                Severity = severity,
                Title = title,
                Message = msg,
                LicensingAssetId = lic.Id,
                EntityType = "LicensingAsset",
                EntityIdentifier = lic.LicenseName
            }, msg, title);
        }
    }

    // ─── CONTRACT EXPIRY ────────────────────────────────────────────────────────

    private async Task CheckContractExpiryAsync(ITAMSDbContext context, IEmailService email)
    {
        var today = DateTime.Today;
        var services = await context.ServiceAssets
            .Where(s => s.ContractEndDate > today)
            .ToListAsync();

        foreach (var svc in services)
        {
            var days = (svc.ContractEndDate - today).Days;
            string? severity = days <= 7 ? "Critical" : days <= 30 ? "High" : null;
            if (severity == null) continue;

            var alertType = "CONTRACT_EXPIRY";
            var exists = await context.SystemAlerts.AnyAsync(a =>
                a.AlertType == alertType && a.ServiceAssetId == svc.Id &&
                !a.IsResolved && a.CreatedAt.Date == today);
            if (exists) continue;

            var title = $"Service contract expiring in {days} day(s): {svc.ServiceName}";
            var msg = BuildEmailBody(title,
                $"Service contract <b>{svc.ServiceName}</b> expires on <b>{svc.ContractEndDate:dd-MMM-yyyy}</b>.");

            await CreateAlertAndNotify(context, email, new SystemAlert
            {
                AlertType = alertType,
                Severity = severity,
                Title = title,
                Message = msg,
                ServiceAssetId = svc.Id,
                EntityType = "ServiceAsset",
                EntityIdentifier = svc.ServiceName
            }, msg, title);
        }
    }

    // ─── REPAIR STUCK ───────────────────────────────────────────────────────────

    private async Task CheckRepairStuckAsync(ITAMSDbContext context, IEmailService email)
    {
        var today = DateTime.Today;
        var repairStatus = await context.AssetStatuses
            .FirstOrDefaultAsync(s => s.StatusName.ToLower().Contains("repair"));
        if (repairStatus == null) return;

        var assets = await context.Assets
            .Include(a => a.Project)
            .Include(a => a.Location)
            .Where(a => a.AssetStatusId == repairStatus.Id)
            .ToListAsync();

        foreach (var asset in assets)
        {
            // Find when it entered repair via maintenance requests
            var latestMaint = await context.MaintenanceRequests
                .Where(m => m.AssetId == asset.Id && m.Status == "Open")
                .OrderByDescending(m => m.CreatedAt)
                .FirstOrDefaultAsync();

            var sinceDate = latestMaint?.CreatedAt ?? asset.UpdatedAt ?? asset.CreatedAt;
            var daysInRepair = (today - sinceDate.Date).Days;
            if (daysInRepair < 7) continue;

            var alertType = "REPAIR_STUCK";
            var exists = await context.SystemAlerts.AnyAsync(a =>
                a.AlertType == alertType && a.AssetId == asset.Id &&
                !a.IsResolved && a.CreatedAt.Date == today);
            if (exists) continue;

            var title = $"Asset stuck in Repair for {daysInRepair} days: {asset.AssetTag}";
            var msg = BuildEmailBody(title,
                $"Asset <b>{asset.AssetTag}</b> ({asset.Make} {asset.Model}) has been in Repair status for <b>{daysInRepair} days</b>.<br>" +
                $"Project: {asset.Project?.Name ?? "N/A"} | Location: {asset.Location?.Name ?? "N/A"}");

            await CreateAlertAndNotify(context, email, new SystemAlert
            {
                AlertType = alertType,
                Severity = daysInRepair >= 30 ? "Critical" : "High",
                Title = title,
                Message = msg,
                AssetId = asset.Id,
                EntityType = "Asset",
                EntityIdentifier = asset.AssetTag
            }, msg, title);
        }
    }

    // ─── COMPLIANCE ─────────────────────────────────────────────────────────────

    private async Task CheckComplianceAsync(ITAMSDbContext context, IEmailService email)
    {
        var today = DateTime.Today;
        // Assets with patch status "Not Patched" or USB blocking "Not Blocked"
        var notPatchedStatus = await context.PatchStatuses
            .FirstOrDefaultAsync(s => s.Name.ToLower().Contains("not patch"));
        var notBlockedStatus = await context.USBBlockingStatuses
            .FirstOrDefaultAsync(s => s.Name.ToLower().Contains("not block") || s.Name.ToLower().Contains("unblock"));

        var query = context.Assets.Include(a => a.Project).AsQueryable();
        if (notPatchedStatus != null)
        {
            var assets = await query.Where(a => a.PatchStatusId == notPatchedStatus.Id).ToListAsync();
            foreach (var asset in assets)
            {
                await CreateComplianceAlert(context, email, asset, "Not Patched", today);
            }
        }
        if (notBlockedStatus != null)
        {
            var assets = await query.Where(a => a.USBBlockingStatusId == notBlockedStatus.Id).ToListAsync();
            foreach (var asset in assets)
            {
                await CreateComplianceAlert(context, email, asset, "USB Not Blocked", today);
            }
        }
    }

    private async Task CreateComplianceAlert(ITAMSDbContext context, IEmailService email,
        Asset asset, string issue, DateTime today)
    {
        var alertType = "COMPLIANCE_FAILURE";
        var exists = await context.SystemAlerts.AnyAsync(a =>
            a.AlertType == alertType && a.AssetId == asset.Id &&
            a.Title.Contains(issue) && !a.IsResolved && a.CreatedAt.Date == today);
        if (exists) return;

        var title = $"Compliance issue — {issue}: {asset.AssetTag}";
        var msg = BuildEmailBody(title,
            $"Asset <b>{asset.AssetTag}</b> ({asset.Make} {asset.Model}) has compliance issue: <b>{issue}</b>.<br>" +
            $"Project: {asset.Project?.Name ?? "N/A"}");

        await CreateAlertAndNotify(context, email, new SystemAlert
        {
            AlertType = alertType,
            Severity = "Medium",
            Title = title,
            Message = msg,
            AssetId = asset.Id,
            EntityType = "Asset",
            EntityIdentifier = asset.AssetTag
        }, msg, title);
    }

    // ─── ESCALATION ─────────────────────────────────────────────────────────────

    private async Task EscalateUnacknowledgedAlertsAsync(ITAMSDbContext context, IEmailService email)
    {
        var escalationDays = 2; // escalate after 2 days unacknowledged
        var cutoff = DateTime.UtcNow.AddDays(-escalationDays);

        var alerts = await context.SystemAlerts
            .Where(a => !a.IsAcknowledged && !a.IsResolved &&
                        a.CreatedAt < cutoff &&
                        a.EscalationLevel < 3 &&
                        (a.LastEscalatedAt == null || a.LastEscalatedAt < cutoff))
            .ToListAsync();

        foreach (var alert in alerts)
        {
            alert.EscalationLevel++;
            alert.LastEscalatedAt = DateTime.UtcNow;

            var targetRole = alert.EscalationLevel switch
            {
                2 => "Admin",        // Project Admin (Viju Joseph etc.)
                3 => "Super Admin",  // Super Admin
                _ => "Project Manager"
            };

            var subject = $"[ESCALATED L{alert.EscalationLevel}] {alert.Title}";
            var body = BuildEmailBody(subject,
                $"This alert has been escalated to Level {alert.EscalationLevel} ({targetRole}) because it was not acknowledged within {escalationDays} days.<br><br>" +
                $"<b>Original Alert:</b> {alert.Title}<br>" +
                $"<b>Created:</b> {alert.CreatedAt:dd-MMM-yyyy HH:mm}<br>" +
                $"<b>Severity:</b> {alert.Severity}");

            await email.SendAlertToRoleAsync(targetRole, subject, body, context);
            _logger.LogInformation("Alert {Id} escalated to level {Level}", alert.Id, alert.EscalationLevel);
        }
    }

    // ─── HELPERS ────────────────────────────────────────────────────────────────

    private async Task CreateAlertAndNotify(ITAMSDbContext context, IEmailService email,
        SystemAlert alert, string emailBody, string subject)
    {
        alert.CreatedAt = DateTime.UtcNow;
        context.SystemAlerts.Add(alert);

        // Send to Project Manager (Level 1) first
        await email.SendAlertToRoleAsync("Project Manager", $"[ITAMS Alert] {subject}", emailBody, context);
        alert.EmailSent = true;
        alert.EmailSentAt = DateTime.UtcNow;
    }

    private static string BuildEmailBody(string title, string content) => $"""
        <html><body style="font-family:Arial,sans-serif;padding:20px;">
        <div style="background:#f44336;color:white;padding:12px 20px;border-radius:4px 4px 0 0;">
            <h2 style="margin:0;">⚠️ ITAMS Alert</h2>
        </div>
        <div style="border:1px solid #ddd;padding:20px;border-radius:0 0 4px 4px;">
            <h3>{title}</h3>
            <p>{content}</p>
            <hr/>
            <small style="color:#888;">This is an automated alert from ITAMS. Please log in to acknowledge or resolve this alert.</small>
        </div>
        </body></html>
        """;
}
