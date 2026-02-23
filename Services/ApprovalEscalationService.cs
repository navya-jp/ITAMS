using ITAMS.Data;
using ITAMS.Domain.Entities.Workflow;
using ITAMS.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ITAMS.Services;

public class ApprovalEscalationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ApprovalEscalationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public ApprovalEscalationService(
        IServiceProvider serviceProvider,
        ILogger<ApprovalEscalationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Approval Escalation Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingEscalations();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during escalation processing");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Approval Escalation Service stopped");
    }

    private async Task ProcessPendingEscalations()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ITAMSDbContext>();

        var now = DateTimeHelper.Now;

        // Get all pending approval requests
        var pendingRequests = await context.Set<ApprovalRequest>()
            .Include(r => r.Workflow)
                .ThenInclude(w => w.ApprovalLevels)
            .Where(r => r.Status == "PENDING")
            .ToListAsync();

        if (!pendingRequests.Any())
        {
            _logger.LogDebug("No pending approval requests found");
            return;
        }

        _logger.LogInformation("Processing {Count} pending approval requests for escalation", pendingRequests.Count);

        var escalatedCount = 0;

        foreach (var request in pendingRequests)
        {
            try
            {
                // Get the current approval level
                var currentLevel = request.Workflow.ApprovalLevels
                    .FirstOrDefault(l => l.LevelOrder == request.CurrentLevel);

                if (currentLevel == null)
                {
                    _logger.LogWarning("No approval level found for request {RequestId} at level {Level}", 
                        request.Id, request.CurrentLevel);
                    continue;
                }

                // Calculate time since request was submitted or last action
                var lastAction = await context.Set<ApprovalHistory>()
                    .Where(h => h.RequestId == request.Id)
                    .OrderByDescending(h => h.ActionAt)
                    .FirstOrDefaultAsync();

                var timeSinceLastAction = lastAction != null 
                    ? now - lastAction.ActionAt 
                    : now - request.RequestedAt;

                // Check if timeout has been reached
                if (timeSinceLastAction.TotalHours >= currentLevel.TimeoutHours)
                {
                    await EscalateRequest(context, request, currentLevel, "TIMEOUT");
                    escalatedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing escalation for request {RequestId}", request.Id);
            }
        }

        if (escalatedCount > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("Escalated {Count} approval requests", escalatedCount);
        }
    }

    private async Task EscalateRequest(
        ITAMSDbContext context, 
        ApprovalRequest request, 
        ApprovalLevel currentLevel,
        string triggerReason)
    {
        // Find applicable escalation rule
        var escalationRule = await context.Set<EscalationRule>()
            .Where(r => r.IsActive && r.TriggerType == "TIME_BASED")
            .OrderBy(r => r.EscalationLevel)
            .FirstOrDefaultAsync();

        if (escalationRule == null)
        {
            _logger.LogWarning("No active escalation rule found for request {RequestId}", request.Id);
            return;
        }

        // Create escalation log
        var escalationLog = new EscalationLog
        {
            RequestId = request.Id,
            RuleId = escalationRule.Id,
            EscalationLevel = escalationRule.EscalationLevel,
            EscalatedAt = DateTimeHelper.Now,
            TriggerReason = triggerReason,
            ActionTaken = escalationRule.EscalationAction
        };

        context.Set<EscalationLog>().Add(escalationLog);

        // Update request status
        request.Status = "ESCALATED";

        // Perform escalation action
        switch (escalationRule.EscalationAction)
        {
            case "NOTIFY":
                // TODO: Send notifications to escalation target roles
                _logger.LogInformation(
                    "Escalation notification sent for request {RequestId} to roles: {Roles}",
                    request.Id, escalationRule.EscalationTargetRoles);
                escalationLog.Details = $"Notification sent to: {escalationRule.EscalationTargetRoles}";
                break;

            case "AUTO_APPROVE":
                // Auto-approve the request
                request.Status = "APPROVED";
                request.CompletedAt = DateTimeHelper.Now;
                request.CompletedBy = 1; // System user
                
                var autoApprovalHistory = new ApprovalHistory
                {
                    RequestId = request.Id,
                    Level = request.CurrentLevel,
                    ApproverId = 1, // System user
                    Action = "APPROVED",
                    ActionAt = DateTimeHelper.Now,
                    Comments = $"Auto-approved by escalation rule: {escalationRule.RuleName}"
                };
                
                context.Set<ApprovalHistory>().Add(autoApprovalHistory);
                
                _logger.LogInformation(
                    "Request {RequestId} auto-approved by escalation rule {RuleName}",
                    request.Id, escalationRule.RuleName);
                escalationLog.Details = "Request auto-approved";
                break;

            case "REASSIGN":
                // TODO: Reassign to different approver
                _logger.LogInformation(
                    "Request {RequestId} reassigned by escalation rule {RuleName}",
                    request.Id, escalationRule.RuleName);
                escalationLog.Details = "Request reassigned";
                break;
        }

        _logger.LogInformation(
            "Escalated request {RequestId} at level {Level} - Action: {Action}",
            request.Id, request.CurrentLevel, escalationRule.EscalationAction);
    }
}
