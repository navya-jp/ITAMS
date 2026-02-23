-- Migration: Create Approval and Escalation Workflow Tables
-- Date: 2026-02-23
-- Description: Creates approval workflow, escalation rules, and tracking tables

-- =============================================
-- 1. Create ApprovalWorkflows Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApprovalWorkflows]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ApprovalWorkflows] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [WorkflowName] NVARCHAR(100) NOT NULL,
        [WorkflowType] NVARCHAR(50) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [TriggerConditions] NVARCHAR(2000) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_ApprovalWorkflows] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_ApprovalWorkflows_Name] UNIQUE ([WorkflowName])
    );
    
    CREATE INDEX [IX_ApprovalWorkflows_WorkflowType] ON [dbo].[ApprovalWorkflows] ([WorkflowType]);
    CREATE INDEX [IX_ApprovalWorkflows_IsActive] ON [dbo].[ApprovalWorkflows] ([IsActive]);
    
    PRINT 'Created ApprovalWorkflows table';
END
GO

-- =============================================
-- 2. Create ApprovalLevels Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApprovalLevels]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ApprovalLevels] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [WorkflowId] INT NOT NULL,
        [LevelOrder] INT NOT NULL,
        [LevelName] NVARCHAR(100) NOT NULL,
        [RequiredApproverRoles] NVARCHAR(500) NOT NULL,
        [TimeoutHours] INT NOT NULL DEFAULT 24,
        [ApprovalType] NVARCHAR(20) NOT NULL DEFAULT 'ANY_ONE',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        CONSTRAINT [PK_ApprovalLevels] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_ApprovalLevels_WorkflowLevel] UNIQUE ([WorkflowId], [LevelOrder]),
        CONSTRAINT [FK_ApprovalLevels_Workflows] FOREIGN KEY ([WorkflowId]) REFERENCES [dbo].[ApprovalWorkflows]([Id]) ON DELETE CASCADE
    );
    
    CREATE INDEX [IX_ApprovalLevels_WorkflowId] ON [dbo].[ApprovalLevels] ([WorkflowId]);
    CREATE INDEX [IX_ApprovalLevels_LevelOrder] ON [dbo].[ApprovalLevels] ([LevelOrder]);
    
    PRINT 'Created ApprovalLevels table';
END
GO

-- =============================================
-- 3. Create ApprovalRequests Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApprovalRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ApprovalRequests] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [WorkflowId] INT NOT NULL,
        [RequestType] NVARCHAR(50) NOT NULL,
        [RequestedBy] INT NOT NULL,
        [RequestedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Status] NVARCHAR(20) NOT NULL DEFAULT 'PENDING',
        [CurrentLevel] INT NOT NULL DEFAULT 1,
        [RequestDetails] NVARCHAR(2000) NULL,
        [AssetId] INT NULL,
        [RejectionReason] NVARCHAR(500) NULL,
        [CompletedAt] DATETIME2 NULL,
        [CompletedBy] INT NULL,
        CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ApprovalRequests_Workflows] FOREIGN KEY ([WorkflowId]) REFERENCES [dbo].[ApprovalWorkflows]([Id]),
        CONSTRAINT [FK_ApprovalRequests_Users] FOREIGN KEY ([RequestedBy]) REFERENCES [dbo].[Users]([Id])
    );
    
    CREATE INDEX [IX_ApprovalRequests_WorkflowId] ON [dbo].[ApprovalRequests] ([WorkflowId]);
    CREATE INDEX [IX_ApprovalRequests_RequestedBy] ON [dbo].[ApprovalRequests] ([RequestedBy]);
    CREATE INDEX [IX_ApprovalRequests_Status] ON [dbo].[ApprovalRequests] ([Status]);
    CREATE INDEX [IX_ApprovalRequests_RequestedAt] ON [dbo].[ApprovalRequests] ([RequestedAt]);
    CREATE INDEX [IX_ApprovalRequests_AssetId] ON [dbo].[ApprovalRequests] ([AssetId]);
    
    PRINT 'Created ApprovalRequests table';
END
GO

-- =============================================
-- 4. Create ApprovalHistories Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApprovalHistories]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ApprovalHistories] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RequestId] INT NOT NULL,
        [Level] INT NOT NULL,
        [ApproverId] INT NOT NULL,
        [Action] NVARCHAR(20) NOT NULL,
        [ActionAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [Comments] NVARCHAR(1000) NULL,
        [IpAddress] NVARCHAR(50) NULL,
        CONSTRAINT [PK_ApprovalHistories] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ApprovalHistories_Requests] FOREIGN KEY ([RequestId]) REFERENCES [dbo].[ApprovalRequests]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ApprovalHistories_Users] FOREIGN KEY ([ApproverId]) REFERENCES [dbo].[Users]([Id])
    );
    
    CREATE INDEX [IX_ApprovalHistories_RequestId] ON [dbo].[ApprovalHistories] ([RequestId]);
    CREATE INDEX [IX_ApprovalHistories_ApproverId] ON [dbo].[ApprovalHistories] ([ApproverId]);
    CREATE INDEX [IX_ApprovalHistories_ActionAt] ON [dbo].[ApprovalHistories] ([ActionAt]);
    
    PRINT 'Created ApprovalHistories table';
END
GO

-- =============================================
-- 5. Create EscalationRules Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EscalationRules]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EscalationRules] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RuleName] NVARCHAR(100) NOT NULL,
        [TriggerType] NVARCHAR(20) NOT NULL DEFAULT 'TIME_BASED',
        [TimeoutHours] INT NULL,
        [EventConditions] NVARCHAR(1000) NULL,
        [EscalationLevel] INT NOT NULL,
        [EscalationTargetRoles] NVARCHAR(500) NOT NULL,
        [EscalationAction] NVARCHAR(20) NOT NULL DEFAULT 'NOTIFY',
        [NotificationTemplate] NVARCHAR(100) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] INT NOT NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] INT NULL,
        CONSTRAINT [PK_EscalationRules] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_EscalationRules_Name] UNIQUE ([RuleName])
    );
    
    CREATE INDEX [IX_EscalationRules_TriggerType] ON [dbo].[EscalationRules] ([TriggerType]);
    CREATE INDEX [IX_EscalationRules_IsActive] ON [dbo].[EscalationRules] ([IsActive]);
    CREATE INDEX [IX_EscalationRules_EscalationLevel] ON [dbo].[EscalationRules] ([EscalationLevel]);
    
    PRINT 'Created EscalationRules table';
END
GO

-- =============================================
-- 6. Create EscalationLogs Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EscalationLogs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EscalationLogs] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [RequestId] INT NOT NULL,
        [RuleId] INT NOT NULL,
        [EscalationLevel] INT NOT NULL,
        [EscalatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [TriggerReason] NVARCHAR(20) NOT NULL,
        [NotificationsSent] NVARCHAR(500) NULL,
        [ActionTaken] NVARCHAR(20) NULL,
        [Details] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_EscalationLogs] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_EscalationLogs_Requests] FOREIGN KEY ([RequestId]) REFERENCES [dbo].[ApprovalRequests]([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_EscalationLogs_Rules] FOREIGN KEY ([RuleId]) REFERENCES [dbo].[EscalationRules]([Id])
    );
    
    CREATE INDEX [IX_EscalationLogs_RequestId] ON [dbo].[EscalationLogs] ([RequestId]);
    CREATE INDEX [IX_EscalationLogs_RuleId] ON [dbo].[EscalationLogs] ([RuleId]);
    CREATE INDEX [IX_EscalationLogs_EscalatedAt] ON [dbo].[EscalationLogs] ([EscalatedAt]);
    
    PRINT 'Created EscalationLogs table';
END
GO

-- =============================================
-- 7. Insert Default Approval Workflows
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[ApprovalWorkflows] WHERE [WorkflowType] = 'ASSET_TRANSFER')
BEGIN
    DECLARE @WorkflowId INT;
    
    -- Asset Transfer Workflow
    INSERT INTO [dbo].[ApprovalWorkflows] ([WorkflowName], [WorkflowType], [Description], [IsActive], [CreatedBy])
    VALUES ('Asset Transfer Approval', 'ASSET_TRANSFER', 'Approval workflow for asset transfers between locations', 1, 1);
    
    SET @WorkflowId = SCOPE_IDENTITY();
    
    INSERT INTO [dbo].[ApprovalLevels] ([WorkflowId], [LevelOrder], [LevelName], [RequiredApproverRoles], [TimeoutHours], [ApprovalType], [CreatedBy])
    VALUES 
        (@WorkflowId, 1, 'Manager Approval', '["Manager"]', 24, 'ANY_ONE', 1),
        (@WorkflowId, 2, 'Admin Approval', '["Admin"]', 48, 'ANY_ONE', 1);
    
    -- Asset Decommission Workflow
    INSERT INTO [dbo].[ApprovalWorkflows] ([WorkflowName], [WorkflowType], [Description], [IsActive], [CreatedBy])
    VALUES ('Asset Decommission Approval', 'ASSET_DECOMMISSION', 'Approval workflow for asset decommissioning', 1, 1);
    
    SET @WorkflowId = SCOPE_IDENTITY();
    
    INSERT INTO [dbo].[ApprovalLevels] ([WorkflowId], [LevelOrder], [LevelName], [RequiredApproverRoles], [TimeoutHours], [ApprovalType], [CreatedBy])
    VALUES 
        (@WorkflowId, 1, 'Manager Approval', '["Manager"]', 24, 'ANY_ONE', 1),
        (@WorkflowId, 2, 'Admin Approval', '["Admin"]', 48, 'ANY_ONE', 1),
        (@WorkflowId, 3, 'SuperAdmin Approval', '["Super Admin"]', 72, 'ANY_ONE', 1);
    
    PRINT 'Inserted default approval workflows';
END
GO

-- =============================================
-- 8. Insert Default Escalation Rules
-- =============================================
IF NOT EXISTS (SELECT * FROM [dbo].[EscalationRules] WHERE [RuleName] = 'Standard Time-Based Escalation')
BEGIN
    INSERT INTO [dbo].[EscalationRules] 
        ([RuleName], [TriggerType], [TimeoutHours], [EscalationLevel], [EscalationTargetRoles], [EscalationAction], [IsActive], [CreatedBy])
    VALUES 
        ('Standard Time-Based Escalation', 'TIME_BASED', 48, 1, '["Admin", "Super Admin"]', 'NOTIFY', 1, 1),
        ('Critical Asset Escalation', 'EVENT_BASED', 24, 2, '["Super Admin"]', 'NOTIFY', 1, 1),
        ('Final Level Auto-Approve', 'TIME_BASED', 72, 3, '["Super Admin"]', 'AUTO_APPROVE', 1, 1);
    
    PRINT 'Inserted default escalation rules';
END
GO

PRINT 'Approval and Escalation Workflow Tables migration completed successfully';
