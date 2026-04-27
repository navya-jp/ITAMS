-- ============================================================
-- ITAMS - IT Asset Management System
-- Complete Database Schema
-- Generated from live database: ITAMS_Shared
-- Date: 27-Apr-2026
-- ============================================================
-- NOTE: Run this script while connected to your target database.
-- Or uncomment and edit the USE statement below:
-- USE ITAMS_neww;
-- GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ApprovalHistories]', 'U') IS NULL
CREATE TABLE [ApprovalHistories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RequestId] int NOT NULL,
    [Level] int NOT NULL,
    [ApproverId] int NOT NULL,
    [Action] nvarchar(20) NOT NULL,
    [ActionAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [Comments] nvarchar(1000) NULL,
    [IpAddress] nvarchar(50) NULL,
    CONSTRAINT [PK_ApprovalHistories] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ApprovalLevels]', 'U') IS NULL
CREATE TABLE [ApprovalLevels] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [WorkflowId] int NOT NULL,
    [LevelOrder] int NOT NULL,
    [LevelName] nvarchar(100) NOT NULL,
    [RequiredApproverRoles] nvarchar(500) NOT NULL,
    [TimeoutHours] int NOT NULL DEFAULT ((24)),
    [ApprovalType] nvarchar(20) NOT NULL DEFAULT ('ANY_ONE'),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    CONSTRAINT [PK_ApprovalLevels] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ApprovalRequests]', 'U') IS NULL
CREATE TABLE [ApprovalRequests] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [WorkflowId] int NOT NULL,
    [RequestType] nvarchar(50) NOT NULL,
    [RequestedBy] int NOT NULL,
    [RequestedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [Status] nvarchar(20) NOT NULL DEFAULT ('PENDING'),
    [CurrentLevel] int NOT NULL DEFAULT ((1)),
    [RequestDetails] nvarchar(2000) NULL,
    [AssetId] int NULL,
    [RejectionReason] nvarchar(500) NULL,
    [CompletedAt] datetime2 NULL,
    [CompletedBy] int NULL,
    CONSTRAINT [PK_ApprovalRequests] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ApprovalWorkflows]', 'U') IS NULL
CREATE TABLE [ApprovalWorkflows] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [WorkflowName] nvarchar(100) NOT NULL,
    [WorkflowType] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [TriggerConditions] nvarchar(2000) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_ApprovalWorkflows] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetAssignmentHistories]', 'U') IS NULL
CREATE TABLE [AssetAssignmentHistories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] int NOT NULL,
    [PreviousUserId] int NULL,
    [PreviousUserName] nvarchar(200) NULL,
    [NewUserId] int NULL,
    [NewUserName] nvarchar(200) NULL,
    [PreviousLocationId] int NULL,
    [PreviousLocationName] nvarchar(200) NULL,
    [NewLocationId] int NULL,
    [NewLocationName] nvarchar(200) NULL,
    [Reason] nvarchar(500) NULL,
    [ChangedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ChangedBy] int NOT NULL DEFAULT ((1)),
    [ChangedByName] nvarchar(200) NULL,
    CONSTRAINT [PK_AssetAssignmentHistories] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetCategories]', 'U') IS NULL
CREATE TABLE [AssetCategories] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CategoryName] nvarchar(100) NOT NULL,
    [CategoryCode] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [Icon] nvarchar(50) NULL,
    [ColorCode] nvarchar(7) NULL,
    [DisplayOrder] int NOT NULL DEFAULT ((0)),
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [IsPredefined] bit NOT NULL DEFAULT ((0)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AssetCategories] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetClassifications]', 'U') IS NULL
CREATE TABLE [AssetClassifications] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ClassificationId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_AssetClassifications] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetMasterFields]', 'U') IS NULL
CREATE TABLE [AssetMasterFields] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FieldName] nvarchar(100) NOT NULL,
    [FieldCode] nvarchar(50) NOT NULL,
    [DataType] nvarchar(50) NOT NULL DEFAULT ('Text'),
    [Description] nvarchar(200) NULL,
    [IsRequired] bit NOT NULL DEFAULT ((0)),
    [DefaultValue] nvarchar(500) NULL,
    [ValidationRules] nvarchar(1000) NULL,
    [MaxLength] int NULL,
    [MinValue] decimal(18,2) NULL,
    [MaxValue] decimal(18,2) NULL,
    [RegexPattern] nvarchar(500) NULL,
    [ValidationMessage] nvarchar(200) NULL,
    [DropdownOptions] nvarchar(1000) NULL,
    [FieldGroup] nvarchar(100) NULL,
    [DisplayOrder] int NOT NULL DEFAULT ((0)),
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [IsSystemField] bit NOT NULL DEFAULT ((0)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AssetMasterFields] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetPlacings]', 'U') IS NULL
CREATE TABLE [AssetPlacings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PlacingId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_AssetPlacings] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Assets]', 'U') IS NULL
CREATE TABLE [Assets] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetTag] nvarchar(50) NOT NULL,
    [ProjectId] int NOT NULL,
    [LocationId] int NOT NULL,
    [UsageCategory] int NOT NULL,
    [AssetType] nvarchar(100) NOT NULL,
    [SubType] nvarchar(100) NULL,
    [Make] nvarchar(100) NOT NULL,
    [Model] nvarchar(100) NOT NULL,
    [SerialNumber] nvarchar(100) NULL,
    [ProcurementDate] datetime2 NULL,
    [ProcurementCost] decimal(18,2) NULL,
    [Vendor] nvarchar(200) NULL,
    [WarrantyStartDate] datetime2 NULL,
    [WarrantyEndDate] datetime2 NULL,
    [CommissioningDate] datetime2 NULL,
    [Status] int NOT NULL,
    [AssignedUserId] int NULL,
    [AssignedUserRole] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] int NOT NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [AssetId] nvarchar(50) NULL,
    [ProjectIdRef] nvarchar(50) NOT NULL,
    [LocationIdRef] nvarchar(50) NOT NULL,
    [AssignedUserIdRef] nvarchar(50) NULL,
    [Region] nvarchar(100) NULL,
    [State] nvarchar(100) NULL,
    [Site] nvarchar(200) NULL,
    [PlazaName] nvarchar(200) NULL,
    [LocationText] nvarchar(200) NULL,
    [Department] nvarchar(100) NULL,
    [Classification] nvarchar(100) NULL,
    [OSType] nvarchar(100) NULL,
    [OSVersion] nvarchar(100) NULL,
    [DBType] nvarchar(100) NULL,
    [DBVersion] nvarchar(100) NULL,
    [AssignedUser] nvarchar(200) NULL,
    [UserRole] nvarchar(100) NULL,
    [ProcuredBy] nvarchar(200) NULL,
    [PatchStatus] nvarchar(100) NULL,
    [USBBlockingStatus] nvarchar(100) NULL,
    [Remarks] nvarchar(MAX) NULL,
    [AssignedUserText] nvarchar(200) NULL,
    [Placing] nvarchar(50) NULL,
    [AssetTypeId] int NULL,
    [AssetSubTypeId] int NULL,
    [AssetStatusId] int NULL,
    [AssetClassificationId] int NULL,
    [OperatingSystemId] int NULL,
    [DatabaseTypeId] int NULL,
    [PatchStatusId] int NULL,
    [USBBlockingStatusId] int NULL,
    [AssetPlacingId] int NULL,
    [VendorId] int NULL,
    [AssetCategoryId] int NULL,
    [CommissioningDateText] nvarchar(100) NULL,
    [ExtraFields] nvarchar(MAX) NULL,
    [IsDecommissioned] bit NOT NULL DEFAULT ((0)),
    [DecommissionedAt] datetime2 NULL,
    [DecommissionedBy] int NULL,
    CONSTRAINT [PK_Assets] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetStatuses]', 'U') IS NULL
CREATE TABLE [AssetStatuses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StatusName] nvarchar(50) NOT NULL,
    [StatusCode] nvarchar(20) NOT NULL,
    [Description] nvarchar(200) NULL,
    [ColorCode] nvarchar(7) NOT NULL DEFAULT ('#808080'),
    [Icon] nvarchar(50) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [IsPredefined] bit NOT NULL DEFAULT ((0)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AssetStatuses] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetSubTypes]', 'U') IS NULL
CREATE TABLE [AssetSubTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TypeId] int NOT NULL,
    [SubTypeName] nvarchar(100) NOT NULL,
    [SubTypeCode] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [DisplayOrder] int NOT NULL DEFAULT ((0)),
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AssetSubTypes] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetTransferRequests]', 'U') IS NULL
CREATE TABLE [AssetTransferRequests] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] int NOT NULL,
    [FromLocationId] int NOT NULL,
    [ToLocationId] int NULL,
    [FromUserId] int NULL,
    [ToUserId] int NULL,
    [Reason] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('Completed'),
    [Notes] nvarchar(500) NULL,
    [TransferDate] datetime2 NOT NULL DEFAULT (getutcdate()),
    [RequestedBy] int NOT NULL DEFAULT ((1)),
    [RequestedByName] nvarchar(200) NULL,
    CONSTRAINT [PK_AssetTransferRequests] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AssetTypes]', 'U') IS NULL
CREATE TABLE [AssetTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [CategoryId] int NOT NULL,
    [TypeName] nvarchar(100) NOT NULL,
    [TypeCode] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NULL,
    [DisplayOrder] int NOT NULL DEFAULT ((0)),
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_AssetTypes] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AuditEntries]', 'U') IS NULL
CREATE TABLE [AuditEntries] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Action] nvarchar(100) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(MAX) NULL,
    [OldValues] nvarchar(MAX) NULL,
    [NewValues] nvarchar(MAX) NULL,
    [Timestamp] datetime2 NOT NULL,
    [UserId] int NOT NULL,
    [IpAddress] nvarchar(MAX) NULL,
    [UserAgent] nvarchar(MAX) NULL,
    [UserIdRef] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AuditEntries] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[AuditLogs]', 'U') IS NULL
CREATE TABLE [AuditLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActionType] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [EntityId] nvarchar(100) NOT NULL,
    [ActorId] int NOT NULL,
    [ActorName] nvarchar(100) NOT NULL,
    [Details] nvarchar(MAX) NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ActorIdRef] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ComplianceChecks]', 'U') IS NULL
CREATE TABLE [ComplianceChecks] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] int NOT NULL,
    [CheckType] nvarchar(100) NOT NULL,
    [Result] nvarchar(20) NOT NULL DEFAULT ('Pass'),
    [Details] nvarchar(500) NULL,
    [Remediation] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('Open'),
    [CheckedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CheckedBy] int NOT NULL DEFAULT ((1)),
    [CheckedByName] nvarchar(200) NULL,
    [ResolvedAt] datetime2 NULL,
    [ResolvedBy] int NULL,
    CONSTRAINT [PK_ComplianceChecks] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ContractTypes]', 'U') IS NULL
CREATE TABLE [ContractTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TypeName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_ContractTypes] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[CriticalityLevels]', 'U') IS NULL
CREATE TABLE [CriticalityLevels] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [LevelName] nvarchar(50) NOT NULL,
    [LevelCode] nvarchar(20) NOT NULL,
    [Description] nvarchar(200) NULL,
    [PriorityOrder] int NOT NULL,
    [SlaHours] int NOT NULL,
    [PriorityLevel] nvarchar(20) NOT NULL DEFAULT ('Medium'),
    [NotificationThresholdDays] int NOT NULL DEFAULT ((30)),
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [IsPredefined] bit NOT NULL DEFAULT ((0)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_CriticalityLevels] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[DatabaseTypes]', 'U') IS NULL
CREATE TABLE [DatabaseTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [DBTypeId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_DatabaseTypes] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[DecommissionArchives]', 'U') IS NULL
CREATE TABLE [DecommissionArchives] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] int NOT NULL,
    [AssetTag] nvarchar(50) NOT NULL,
    [AssetSnapshot] nvarchar(MAX) NOT NULL,
    [DecommissionReason] nvarchar(2000) NOT NULL,
    [DisposalMethod] nvarchar(100) NOT NULL,
    [Notes] nvarchar(2000) NULL,
    [ApprovalRequestId] int NOT NULL,
    [ApprovalChainSnapshot] nvarchar(MAX) NOT NULL,
    [ArchivedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ArchivedBy] int NOT NULL,
    [ArchivedByName] nvarchar(200) NULL,
    CONSTRAINT [PK_DecommissionArchives] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[EscalationLogs]', 'U') IS NULL
CREATE TABLE [EscalationLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RequestId] int NOT NULL,
    [RuleId] int NOT NULL,
    [EscalationLevel] int NOT NULL,
    [EscalatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [TriggerReason] nvarchar(20) NOT NULL,
    [NotificationsSent] nvarchar(500) NULL,
    [ActionTaken] nvarchar(20) NULL,
    [Details] nvarchar(1000) NULL,
    CONSTRAINT [PK_EscalationLogs] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[EscalationRules]', 'U') IS NULL
CREATE TABLE [EscalationRules] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RuleName] nvarchar(100) NOT NULL,
    [TriggerType] nvarchar(20) NOT NULL DEFAULT ('TIME_BASED'),
    [TimeoutHours] int NULL,
    [EventConditions] nvarchar(1000) NULL,
    [EscalationLevel] int NOT NULL,
    [EscalationTargetRoles] nvarchar(500) NOT NULL,
    [EscalationAction] nvarchar(20) NOT NULL DEFAULT ('NOTIFY'),
    [NotificationTemplate] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_EscalationRules] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[LicensingAssets]', 'U') IS NULL
CREATE TABLE [LicensingAssets] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [LicenseName] nvarchar(200) NOT NULL,
    [Version] nvarchar(50) NOT NULL,
    [LicenseKey] nvarchar(500) NOT NULL,
    [LicenseType] nvarchar(50) NOT NULL,
    [NumberOfLicenses] int NOT NULL,
    [PurchaseDate] datetime2 NOT NULL,
    [ValidityStartDate] datetime2 NOT NULL,
    [ValidityEndDate] datetime2 NOT NULL,
    [ValidityType] nvarchar(50) NOT NULL,
    [Vendor] nvarchar(100) NOT NULL,
    [Publisher] nvarchar(100) NOT NULL,
    [AssetTag] nvarchar(50) NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    [AssetId] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_LicensingAssets] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Locations]', 'U') IS NULL
CREATE TABLE [Locations] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Region] nvarchar(100) NOT NULL,
    [State] nvarchar(100) NOT NULL,
    [Site] nvarchar(100) NULL,
    [Lane] nvarchar(100) NULL,
    [Office] nvarchar(100) NULL,
    [Address] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [ProjectId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [LocationId] nvarchar(50) NULL,
    [ProjectIdRef] nvarchar(50) NOT NULL,
    [IsSensitive] bit NOT NULL DEFAULT ((0)),
    [SensitiveReason] nvarchar(500) NULL,
    CONSTRAINT [PK_Locations] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[LoginAudit]', 'U') IS NULL
CREATE TABLE [LoginAudit] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [LoginTime] datetime2 NOT NULL DEFAULT (getutcdate()),
    [IpAddress] nvarchar(50) NULL,
    [BrowserType] nvarchar(200) NULL,
    [OperatingSystem] nvarchar(200) NULL,
    [LogoutTime] datetime2 NULL,
    [Status] nvarchar(50) NOT NULL DEFAULT ('ACTIVE'),
    [SessionId] nvarchar(500) NULL,
    [SessionStatusId] int NULL,
    CONSTRAINT [PK_LoginAudit] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[MaintenanceRequests]', 'U') IS NULL
CREATE TABLE [MaintenanceRequests] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] int NOT NULL,
    [RequestType] nvarchar(50) NOT NULL DEFAULT ('Maintenance'),
    [Description] nvarchar(500) NOT NULL DEFAULT (''),
    [OldSpecifications] nvarchar(1000) NULL,
    [NewSpecifications] nvarchar(1000) NULL,
    [VendorName] nvarchar(200) NULL,
    [Cost] decimal(18,2) NULL,
    [ScheduledDate] datetime2 NULL,
    [CompletedDate] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('Open'),
    [Resolution] nvarchar(500) NULL,
    [Remarks] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL DEFAULT ((1)),
    [CreatedByName] nvarchar(200) NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_MaintenanceRequests] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[OperatingSystems]', 'U') IS NULL
CREATE TABLE [OperatingSystems] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [OSId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_OperatingSystems] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[PatchStatuses]', 'U') IS NULL
CREATE TABLE [PatchStatuses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StatusId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_PatchStatuses] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Permissions]', 'U') IS NULL
CREATE TABLE [Permissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Code] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Module] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [PermissionId] nvarchar(50) NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Projects]', 'U') IS NULL
CREATE TABLE [Projects] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Code] nvarchar(50) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] int NOT NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [PreferredName] nvarchar(100) NULL,
    [States] nvarchar(200) NULL,
    [SpvName] nvarchar(200) NULL,
    [ProjectId] nvarchar(50) NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacAccessAuditLog]', 'U') IS NULL
CREATE TABLE [RbacAccessAuditLog] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [PermissionCode] nvarchar(100) NOT NULL,
    [ResourceId] nvarchar(100) NULL,
    [ResourceType] nvarchar(50) NULL,
    [ActionAttempted] nvarchar(50) NULL,
    [AccessGranted] bit NOT NULL,
    [DenialReason] nvarchar(500) NULL,
    [ResolutionMethod] nvarchar(50) NULL,
    [ScopeValidated] bit NOT NULL DEFAULT ((0)),
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [SessionId] nvarchar(100) NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (getutcdate()),
    [UserIdRef] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_RbacAccessAuditLog] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacPermissionAuditLog]', 'U') IS NULL
CREATE TABLE [RbacPermissionAuditLog] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActorUserId] int NOT NULL,
    [TargetUserId] int NULL,
    [RoleId] int NULL,
    [PermissionId] int NULL,
    [ActionType] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [OldValue] nvarchar(MAX) NULL,
    [NewValue] nvarchar(MAX) NULL,
    [Reason] nvarchar(500) NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [SessionId] nvarchar(100) NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ActorUserIdRef] nvarchar(50) NOT NULL,
    [TargetUserIdRef] nvarchar(50) NULL,
    [RoleIdRef] nvarchar(50) NULL,
    [PermissionIdRef] nvarchar(50) NULL,
    CONSTRAINT [PK_RbacPermissionAuditLog] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacPermissions]', 'U') IS NULL
CREATE TABLE [RbacPermissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Code] nvarchar(100) NOT NULL,
    [Module] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [ResourceType] nvarchar(50) NOT NULL,
    [Action] nvarchar(50) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [DeactivatedAt] datetime2 NULL,
    [DeactivatedBy] int NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [RbacPermissionId] nvarchar(50) NULL,
    [CreatedByRef] nvarchar(50) NOT NULL,
    [DeactivatedByRef] nvarchar(50) NULL,
    CONSTRAINT [PK_RbacPermissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacRolePermissions]', 'U') IS NULL
CREATE TABLE [RbacRolePermissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [Allowed] bit NOT NULL DEFAULT ((1)),
    [GrantedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [GrantedBy] int NOT NULL,
    [RevokedAt] datetime2 NULL,
    [RevokedBy] int NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [RoleIdRef] nvarchar(50) NOT NULL,
    [PermissionIdRef] nvarchar(50) NOT NULL,
    [GrantedByRef] nvarchar(50) NOT NULL,
    [RevokedByRef] nvarchar(50) NULL,
    CONSTRAINT [PK_RbacRolePermissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacRoles]', 'U') IS NULL
CREATE TABLE [RbacRoles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsSystemRole] bit NOT NULL DEFAULT ((0)),
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [DeactivatedAt] datetime2 NULL,
    [DeactivatedBy] int NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [RbacRoleId] nvarchar(50) NULL,
    [CreatedByRef] nvarchar(50) NOT NULL,
    [DeactivatedByRef] nvarchar(50) NULL,
    CONSTRAINT [PK_RbacRoles] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacUserPermissions]', 'U') IS NULL
CREATE TABLE [RbacUserPermissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [Allowed] bit NOT NULL,
    [GrantedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [GrantedBy] int NOT NULL,
    [RevokedAt] datetime2 NULL,
    [RevokedBy] int NULL,
    [Reason] nvarchar(500) NULL,
    [ExpiresAt] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [UserIdRef] nvarchar(50) NOT NULL,
    [PermissionIdRef] nvarchar(50) NOT NULL,
    [GrantedByRef] nvarchar(50) NULL,
    [RevokedByRef] nvarchar(50) NULL,
    CONSTRAINT [PK_RbacUserPermissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RbacUserScope]', 'U') IS NULL
CREATE TABLE [RbacUserScope] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [ScopeType] nvarchar(20) NOT NULL,
    [ProjectId] int NULL,
    [AssignedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [AssignedBy] int NOT NULL,
    [RemovedAt] datetime2 NULL,
    [RemovedBy] int NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [UserIdRef] nvarchar(50) NOT NULL,
    [ProjectIdRef] nvarchar(50) NULL,
    [AssignedByRef] nvarchar(50) NOT NULL,
    [RemovedByRef] nvarchar(50) NULL,
    CONSTRAINT [PK_RbacUserScope] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[RolePermissions]', 'U') IS NULL
CREATE TABLE [RolePermissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [IsGranted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [RoleIdRef] nvarchar(50) NOT NULL,
    [PermissionIdRef] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Roles]', 'U') IS NULL
CREATE TABLE [Roles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsSystemRole] bit NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [DeactivatedAt] datetime2 NULL,
    [DeactivatedByUserId] int NULL,
    [DeactivationReason] nvarchar(500) NULL,
    [ScopeType] nvarchar(20) NOT NULL DEFAULT ('PROJECT'),
    [CreatedBy] int NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [RoleId] nvarchar(50) NULL,
    [CreatedByRef] nvarchar(50) NULL,
    [DeactivatedByUserIdRef] nvarchar(50) NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[SecurityAlerts]', 'U') IS NULL
CREATE TABLE [SecurityAlerts] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AlertType] nvarchar(100) NOT NULL,
    [Severity] nvarchar(20) NOT NULL DEFAULT ('MEDIUM'),
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(MAX) NOT NULL,
    [UserId] int NULL,
    [Username] nvarchar(100) NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [AuditLogId] int NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('OPEN'),
    [AssignedToUserId] int NULL,
    [Notes] nvarchar(MAX) NULL,
    [Resolution] nvarchar(MAX) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ResolvedAt] datetime2 NULL,
    [Metadata] nvarchar(MAX) NULL,
    [IsAutoGenerated] bit NOT NULL DEFAULT ((1)),
    [UserIdRef] nvarchar(50) NOT NULL,
    [AssignedToUserIdRef] nvarchar(50) NULL,
    [AuditLogIdRef] nvarchar(50) NULL,
    CONSTRAINT [PK_SecurityAlerts] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[SecurityAuditLogs]', 'U') IS NULL
CREATE TABLE [SecurityAuditLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ActorUserId] int NULL,
    [ActorUsername] nvarchar(100) NOT NULL,
    [ActionType] nvarchar(100) NOT NULL,
    [EntityType] nvarchar(100) NOT NULL,
    [EntityId] nvarchar(100) NULL,
    [EntityName] nvarchar(200) NULL,
    [TargetUserId] int NULL,
    [TargetUsername] nvarchar(100) NULL,
    [OldValues] nvarchar(MAX) NULL,
    [NewValues] nvarchar(MAX) NULL,
    [Reason] nvarchar(1000) NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [SessionId] nvarchar(100) NULL,
    [RiskLevel] nvarchar(20) NOT NULL DEFAULT ('LOW'),
    [Success] bit NOT NULL DEFAULT ((1)),
    [ErrorMessage] nvarchar(1000) NULL,
    [Metadata] nvarchar(MAX) NULL,
    [Timestamp] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ComplianceFlags] nvarchar(200) NULL,
    [ActorUserIdRef] nvarchar(50) NOT NULL,
    [TargetUserIdRef] nvarchar(50) NULL,
    [SecurityAuditLogId] nvarchar(50) NULL,
    CONSTRAINT [PK_SecurityAuditLogs] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ServiceAssets]', 'U') IS NULL
CREATE TABLE [ServiceAssets] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetId] nvarchar(50) NOT NULL,
    [ServiceName] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [ServiceTypeId] int NOT NULL,
    [ContractTypeId] int NOT NULL,
    [Vendor] nvarchar(100) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [RenewalCycle] nvarchar(50) NOT NULL,
    [RenewalReminderDays] int NOT NULL,
    [LastRenewalDate] datetime2 NULL,
    [NextRenewalDate] datetime2 NULL,
    [Status] nvarchar(50) NOT NULL,
    [ContractValue] decimal(18,2) NULL,
    [ContractNumber] nvarchar(500) NULL,
    [Remarks] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    [VendorName] nvarchar(200) NOT NULL DEFAULT (''),
    [ContractStartDate] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ContractEndDate] datetime2 NOT NULL DEFAULT (getutcdate()),
    [RenewalCycleMonths] int NOT NULL DEFAULT ((12)),
    [ContractCost] decimal(18,2) NULL,
    [BillingCycle] nvarchar(50) NULL,
    [Currency] nvarchar(10) NOT NULL DEFAULT ('INR'),
    [SLAType] nvarchar(100) NULL,
    [ResponseTime] nvarchar(100) NULL,
    [CoverageDetails] nvarchar(500) NULL,
    [ContactPerson] nvarchar(200) NULL,
    [SupportContactNumber] nvarchar(50) NULL,
    [UsageCategory] nvarchar(50) NOT NULL DEFAULT ('TMS'),
    [AutoRenewEnabled] bit NOT NULL DEFAULT ((0)),
    [ProjectId] int NULL,
    [LocationId] int NULL,
    [VendorId] int NULL,
    CONSTRAINT [PK_ServiceAssets] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ServiceRenewals]', 'U') IS NULL
CREATE TABLE [ServiceRenewals] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [ServiceId] int NOT NULL,
    [PreviousEndDate] datetime2 NOT NULL,
    [NewStartDate] datetime2 NOT NULL,
    [NewEndDate] datetime2 NOT NULL,
    [RenewalCost] decimal(18,2) NULL,
    [RenewedBy] int NOT NULL,
    [RenewalDate] datetime2 NOT NULL DEFAULT (getutcdate()),
    [Remarks] nvarchar(500) NULL,
    CONSTRAINT [PK_ServiceRenewals] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[ServiceTypes]', 'U') IS NULL
CREATE TABLE [ServiceTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TypeName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_ServiceTypes] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[SessionStatuses]', 'U') IS NULL
CREATE TABLE [SessionStatuses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StatusId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_SessionStatuses] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[SystemAlerts]', 'U') IS NULL
CREATE TABLE [SystemAlerts] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AlertType] nvarchar(50) NOT NULL,
    [Severity] nvarchar(50) NOT NULL DEFAULT ('Medium'),
    [Title] nvarchar(500) NOT NULL,
    [Message] nvarchar(2000) NULL,
    [AssetId] int NULL,
    [LicensingAssetId] int NULL,
    [ServiceAssetId] int NULL,
    [EntityType] nvarchar(100) NULL,
    [EntityIdentifier] nvarchar(100) NULL,
    [IsAcknowledged] bit NOT NULL DEFAULT ((0)),
    [AcknowledgedBy] int NULL,
    [AcknowledgedAt] datetime2 NULL,
    [EmailSent] bit NOT NULL DEFAULT ((0)),
    [EmailSentAt] datetime2 NULL,
    [EscalationLevel] int NOT NULL DEFAULT ((1)),
    [LastEscalatedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ResolvedAt] datetime2 NULL,
    [IsResolved] bit NOT NULL DEFAULT ((0)),
    CONSTRAINT [PK_SystemAlerts] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[SystemSettings]', 'U') IS NULL
CREATE TABLE [SystemSettings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SettingKey] nvarchar(100) NOT NULL,
    [SettingValue] nvarchar(500) NOT NULL,
    [Description] nvarchar(500) NULL,
    [Category] nvarchar(50) NOT NULL DEFAULT ('General'),
    [DataType] nvarchar(20) NOT NULL DEFAULT ('String'),
    [IsEditable] bit NOT NULL DEFAULT ((1)),
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[TypeFieldMappings]', 'U') IS NULL
CREATE TABLE [TypeFieldMappings] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [AssetTypeId] int NOT NULL,
    [FieldId] int NOT NULL,
    [IsRequired] bit NOT NULL DEFAULT ((0)),
    [DefaultValue] nvarchar(500) NULL,
    [DisplayOrder] int NOT NULL DEFAULT ((0)),
    [IsVisible] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    CONSTRAINT [PK_TypeFieldMappings] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[USBBlockingStatuses]', 'U') IS NULL
CREATE TABLE [USBBlockingStatuses] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StatusId] nvarchar(50) NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    CONSTRAINT [PK_USBBlockingStatuses] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[UserPermissions]', 'U') IS NULL
CREATE TABLE [UserPermissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [IsGranted] bit NOT NULL,
    [ScopeType] nvarchar(20) NOT NULL DEFAULT ('GLOBAL'),
    [ProjectId] int NULL,
    [GrantedByUserId] int NOT NULL,
    [Reason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [ExpiresAt] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [UserIdRef] nvarchar(50) NOT NULL,
    [ProjectIdRef] nvarchar(50) NOT NULL,
    [PermissionIdRef] nvarchar(50) NOT NULL,
    [GrantedByUserIdRef] nvarchar(50) NULL,
    CONSTRAINT [PK_UserPermissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[UserProjectPermissions]', 'U') IS NULL
CREATE TABLE [UserProjectPermissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserProjectId] int NOT NULL,
    [PermissionId] int NOT NULL,
    [IsGranted] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [GrantedBy] int NOT NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [PermissionIdRef] nvarchar(50) NOT NULL,
    [UserProjectIdRef] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_UserProjectPermissions] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[UserProjects]', 'U') IS NULL
CREATE TABLE [UserProjects] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [ProjectId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [AssignedAt] datetime2 NOT NULL,
    [AssignedBy] int NOT NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [UserIdRef] nvarchar(50) NOT NULL,
    [ProjectIdRef] nvarchar(50) NOT NULL,
    [UserProjectId] nvarchar(50) NULL,
    CONSTRAINT [PK_UserProjects] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Users]', 'U') IS NULL
CREATE TABLE [Users] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Username] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(MAX) NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [RoleId] int NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [LastLoginAt] datetime2 NULL,
    [PasswordChangedAt] datetime2 NULL,
    [MustChangePassword] bit NOT NULL,
    [FailedLoginAttempts] int NOT NULL,
    [LockedUntil] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [DeactivatedAt] datetime2 NULL,
    [DeactivatedByUserId] int NULL,
    [DeactivationReason] nvarchar(500) NULL,
    [LastLoginIp] nvarchar(45) NULL,
    [SessionTimeoutMinutes] int NOT NULL DEFAULT ((0)),
    [CreatedBy] int NULL,
    [DeactivatedBy] int NULL,
    [IPAddress] nvarchar(45) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NULL,
    [UserId] nvarchar(50) NOT NULL,
    [RoleIdRef] nvarchar(50) NULL,
    [CreatedByRef] nvarchar(50) NULL,
    [DeactivatedByRef] nvarchar(50) NULL,
    [DeactivatedByUserIdRef] nvarchar(50) NULL,
    [LastActivityAt] datetime2 NULL,
    [ActiveSessionId] nvarchar(500) NULL,
    [SessionStartedAt] datetime2 NULL,
    [ProjectId] int NULL,
    [RestrictedRegion] nvarchar(100) NULL,
    [RestrictedState] nvarchar(100) NULL,
    [RestrictedPlaza] nvarchar(100) NULL,
    [RestrictedOffice] nvarchar(100) NULL,
    [PasswordResetRequested] bit NOT NULL DEFAULT ((0)),
    [PasswordResetRequestedAt] datetime2 NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[UserScopes]', 'U') IS NULL
CREATE TABLE [UserScopes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [ScopeType] nvarchar(20) NOT NULL DEFAULT ('PROJECT'),
    [ProjectId] int NULL,
    [AssignedByUserId] int NOT NULL,
    [Reason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [Status] nvarchar(20) NOT NULL DEFAULT ('ACTIVE'),
    [UserIdRef] nvarchar(50) NOT NULL,
    [ProjectIdRef] nvarchar(50) NULL,
    [AssignedByUserIdRef] nvarchar(50) NULL,
    CONSTRAINT [PK_UserScopes] PRIMARY KEY ([Id])
);
GO

-- ------------------------------------------------------------
IF OBJECT_ID('[Vendors]', 'U') IS NULL
CREATE TABLE [Vendors] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [VendorName] nvarchar(100) NOT NULL,
    [VendorCode] nvarchar(50) NOT NULL,
    [ContactPerson] nvarchar(100) NULL,
    [Email] nvarchar(100) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [Address] nvarchar(500) NULL,
    [Website] nvarchar(200) NULL,
    [IsActive] bit NOT NULL DEFAULT ((1)),
    [IsDeleted] bit NOT NULL DEFAULT ((0)),
    [CreatedAt] datetime2 NOT NULL DEFAULT (getutcdate()),
    [CreatedBy] int NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [UpdatedBy] int NULL,
    CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
);
GO

-- ============================================================
-- FOREIGN KEY CONSTRAINTS
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApprovalHistories_Requests')
    ALTER TABLE [ApprovalHistories] ADD CONSTRAINT [FK_ApprovalHistories_Requests]
    FOREIGN KEY ([RequestId]) REFERENCES [ApprovalRequests]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApprovalHistories_Users')
    ALTER TABLE [ApprovalHistories] ADD CONSTRAINT [FK_ApprovalHistories_Users]
    FOREIGN KEY ([ApproverId]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApprovalLevels_Workflows')
    ALTER TABLE [ApprovalLevels] ADD CONSTRAINT [FK_ApprovalLevels_Workflows]
    FOREIGN KEY ([WorkflowId]) REFERENCES [ApprovalWorkflows]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApprovalRequests_Workflows')
    ALTER TABLE [ApprovalRequests] ADD CONSTRAINT [FK_ApprovalRequests_Workflows]
    FOREIGN KEY ([WorkflowId]) REFERENCES [ApprovalWorkflows]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ApprovalRequests_Users')
    ALTER TABLE [ApprovalRequests] ADD CONSTRAINT [FK_ApprovalRequests_Users]
    FOREIGN KEY ([RequestedBy]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssignmentHistory_PrevUser')
    ALTER TABLE [AssetAssignmentHistories] ADD CONSTRAINT [FK_AssignmentHistory_PrevUser]
    FOREIGN KEY ([PreviousUserId]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssignmentHistory_NewUser')
    ALTER TABLE [AssetAssignmentHistories] ADD CONSTRAINT [FK_AssignmentHistory_NewUser]
    FOREIGN KEY ([NewUserId]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssignmentHistory_Assets')
    ALTER TABLE [AssetAssignmentHistories] ADD CONSTRAINT [FK_AssignmentHistory_Assets]
    FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssignmentHistory_PrevLoc')
    ALTER TABLE [AssetAssignmentHistories] ADD CONSTRAINT [FK_AssignmentHistory_PrevLoc]
    FOREIGN KEY ([PreviousLocationId]) REFERENCES [Locations]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssignmentHistory_NewLoc')
    ALTER TABLE [AssetAssignmentHistories] ADD CONSTRAINT [FK_AssignmentHistory_NewLoc]
    FOREIGN KEY ([NewLocationId]) REFERENCES [Locations]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetTypes')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_AssetTypes]
    FOREIGN KEY ([AssetTypeId]) REFERENCES [AssetTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetStatuses')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_AssetStatuses]
    FOREIGN KEY ([AssetStatusId]) REFERENCES [AssetStatuses]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetSubTypes')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_AssetSubTypes]
    FOREIGN KEY ([AssetSubTypeId]) REFERENCES [AssetSubTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetCategory')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_AssetCategory]
    FOREIGN KEY ([AssetCategoryId]) REFERENCES [AssetCategories]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_USBBlockingStatuses')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_USBBlockingStatuses]
    FOREIGN KEY ([USBBlockingStatusId]) REFERENCES [USBBlockingStatuses]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_DatabaseTypes')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_DatabaseTypes]
    FOREIGN KEY ([DatabaseTypeId]) REFERENCES [DatabaseTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_OperatingSystems')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_OperatingSystems]
    FOREIGN KEY ([OperatingSystemId]) REFERENCES [OperatingSystems]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_PatchStatuses')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_PatchStatuses]
    FOREIGN KEY ([PatchStatusId]) REFERENCES [PatchStatuses]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetClassifications')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_AssetClassifications]
    FOREIGN KEY ([AssetClassificationId]) REFERENCES [AssetClassifications]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_AssetPlacings')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_AssetPlacings]
    FOREIGN KEY ([AssetPlacingId]) REFERENCES [AssetPlacings]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_Vendors')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_Vendors]
    FOREIGN KEY ([VendorId]) REFERENCES [Vendors]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_Locations_LocationIdRef')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_Locations_LocationIdRef]
    FOREIGN KEY ([LocationIdRef]) REFERENCES [Locations]([LocationId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Assets_Projects_ProjectIdRef')
    ALTER TABLE [Assets] ADD CONSTRAINT [FK_Assets_Projects_ProjectIdRef]
    FOREIGN KEY ([ProjectIdRef]) REFERENCES [Projects]([ProjectId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetSubTypes_Types')
    ALTER TABLE [AssetSubTypes] ADD CONSTRAINT [FK_AssetSubTypes_Types]
    FOREIGN KEY ([TypeId]) REFERENCES [AssetTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_FromLoc')
    ALTER TABLE [AssetTransferRequests] ADD CONSTRAINT [FK_Transfer_FromLoc]
    FOREIGN KEY ([FromLocationId]) REFERENCES [Locations]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_ToLoc')
    ALTER TABLE [AssetTransferRequests] ADD CONSTRAINT [FK_Transfer_ToLoc]
    FOREIGN KEY ([ToLocationId]) REFERENCES [Locations]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_Assets')
    ALTER TABLE [AssetTransferRequests] ADD CONSTRAINT [FK_Transfer_Assets]
    FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_FromUser')
    ALTER TABLE [AssetTransferRequests] ADD CONSTRAINT [FK_Transfer_FromUser]
    FOREIGN KEY ([FromUserId]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Transfer_ToUser')
    ALTER TABLE [AssetTransferRequests] ADD CONSTRAINT [FK_Transfer_ToUser]
    FOREIGN KEY ([ToUserId]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssetTypes_Categories')
    ALTER TABLE [AssetTypes] ADD CONSTRAINT [FK_AssetTypes_Categories]
    FOREIGN KEY ([CategoryId]) REFERENCES [AssetCategories]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Compliance_Assets')
    ALTER TABLE [ComplianceChecks] ADD CONSTRAINT [FK_Compliance_Assets]
    FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DecommissionArchives_Assets')
    ALTER TABLE [DecommissionArchives] ADD CONSTRAINT [FK_DecommissionArchives_Assets]
    FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EscalationLogs_Rules')
    ALTER TABLE [EscalationLogs] ADD CONSTRAINT [FK_EscalationLogs_Rules]
    FOREIGN KEY ([RuleId]) REFERENCES [EscalationRules]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EscalationLogs_Requests')
    ALTER TABLE [EscalationLogs] ADD CONSTRAINT [FK_EscalationLogs_Requests]
    FOREIGN KEY ([RequestId]) REFERENCES [ApprovalRequests]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Locations_Projects_ProjectIdRef')
    ALTER TABLE [Locations] ADD CONSTRAINT [FK_Locations_Projects_ProjectIdRef]
    FOREIGN KEY ([ProjectIdRef]) REFERENCES [Projects]([ProjectId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LoginAudit_Users')
    ALTER TABLE [LoginAudit] ADD CONSTRAINT [FK_LoginAudit_Users]
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LoginAudit_SessionStatuses')
    ALTER TABLE [LoginAudit] ADD CONSTRAINT [FK_LoginAudit_SessionStatuses]
    FOREIGN KEY ([SessionStatusId]) REFERENCES [SessionStatuses]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Maintenance_Assets')
    ALTER TABLE [MaintenanceRequests] ADD CONSTRAINT [FK_Maintenance_Assets]
    FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RbacPermissionAuditLog_RbacPermissions_PermissionIdRef')
    ALTER TABLE [RbacPermissionAuditLog] ADD CONSTRAINT [FK_RbacPermissionAuditLog_RbacPermissions_PermissionIdRef]
    FOREIGN KEY ([PermissionIdRef]) REFERENCES [RbacPermissions]([RbacPermissionId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RbacPermissionAuditLog_RbacRoles_RoleIdRef')
    ALTER TABLE [RbacPermissionAuditLog] ADD CONSTRAINT [FK_RbacPermissionAuditLog_RbacRoles_RoleIdRef]
    FOREIGN KEY ([RoleIdRef]) REFERENCES [RbacRoles]([RbacRoleId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RbacRolePermissions_Roles_RoleIdRef')
    ALTER TABLE [RbacRolePermissions] ADD CONSTRAINT [FK_RbacRolePermissions_Roles_RoleIdRef]
    FOREIGN KEY ([RoleIdRef]) REFERENCES [RbacRoles]([RbacRoleId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RbacRolePermissions_Permissions_PermissionIdRef')
    ALTER TABLE [RbacRolePermissions] ADD CONSTRAINT [FK_RbacRolePermissions_Permissions_PermissionIdRef]
    FOREIGN KEY ([PermissionIdRef]) REFERENCES [RbacPermissions]([RbacPermissionId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RbacUserPermissions_Permissions_PermissionIdRef')
    ALTER TABLE [RbacUserPermissions] ADD CONSTRAINT [FK_RbacUserPermissions_Permissions_PermissionIdRef]
    FOREIGN KEY ([PermissionIdRef]) REFERENCES [RbacPermissions]([RbacPermissionId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RbacUserScope_Projects_ProjectIdRef')
    ALTER TABLE [RbacUserScope] ADD CONSTRAINT [FK_RbacUserScope_Projects_ProjectIdRef]
    FOREIGN KEY ([ProjectIdRef]) REFERENCES [Projects]([ProjectId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Permissions_PermissionIdRef')
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Permissions_PermissionIdRef]
    FOREIGN KEY ([PermissionIdRef]) REFERENCES [Permissions]([PermissionId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RolePermissions_Roles_RoleIdRef')
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Roles_RoleIdRef]
    FOREIGN KEY ([RoleIdRef]) REFERENCES [Roles]([RoleId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ServiceAssets_ContractType')
    ALTER TABLE [ServiceAssets] ADD CONSTRAINT [FK_ServiceAssets_ContractType]
    FOREIGN KEY ([ContractTypeId]) REFERENCES [ContractTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ServiceAssets_ServiceType')
    ALTER TABLE [ServiceAssets] ADD CONSTRAINT [FK_ServiceAssets_ServiceType]
    FOREIGN KEY ([ServiceTypeId]) REFERENCES [ServiceTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ServiceRenewals_ServiceAssets')
    ALTER TABLE [ServiceRenewals] ADD CONSTRAINT [FK_ServiceRenewals_ServiceAssets]
    FOREIGN KEY ([ServiceId]) REFERENCES [ServiceAssets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SystemAlerts_ServiceAssets')
    ALTER TABLE [SystemAlerts] ADD CONSTRAINT [FK_SystemAlerts_ServiceAssets]
    FOREIGN KEY ([ServiceAssetId]) REFERENCES [ServiceAssets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SystemAlerts_LicensingAssets')
    ALTER TABLE [SystemAlerts] ADD CONSTRAINT [FK_SystemAlerts_LicensingAssets]
    FOREIGN KEY ([LicensingAssetId]) REFERENCES [LicensingAssets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SystemAlerts_Assets')
    ALTER TABLE [SystemAlerts] ADD CONSTRAINT [FK_SystemAlerts_Assets]
    FOREIGN KEY ([AssetId]) REFERENCES [Assets]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TypeFieldMappings_AssetTypes')
    ALTER TABLE [TypeFieldMappings] ADD CONSTRAINT [FK_TypeFieldMappings_AssetTypes]
    FOREIGN KEY ([AssetTypeId]) REFERENCES [AssetTypes]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TypeFieldMappings_Fields')
    ALTER TABLE [TypeFieldMappings] ADD CONSTRAINT [FK_TypeFieldMappings_Fields]
    FOREIGN KEY ([FieldId]) REFERENCES [AssetMasterFields]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_Projects_ProjectIdRef')
    ALTER TABLE [UserPermissions] ADD CONSTRAINT [FK_UserPermissions_Projects_ProjectIdRef]
    FOREIGN KEY ([ProjectIdRef]) REFERENCES [Projects]([ProjectId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserPermissions_Permissions_PermissionIdRef')
    ALTER TABLE [UserPermissions] ADD CONSTRAINT [FK_UserPermissions_Permissions_PermissionIdRef]
    FOREIGN KEY ([PermissionIdRef]) REFERENCES [Permissions]([PermissionId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserProjectPermissions_Permissions_PermissionIdRef')
    ALTER TABLE [UserProjectPermissions] ADD CONSTRAINT [FK_UserProjectPermissions_Permissions_PermissionIdRef]
    FOREIGN KEY ([PermissionIdRef]) REFERENCES [Permissions]([PermissionId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserProjects_Projects_ProjectIdRef')
    ALTER TABLE [UserProjects] ADD CONSTRAINT [FK_UserProjects_Projects_ProjectIdRef]
    FOREIGN KEY ([ProjectIdRef]) REFERENCES [Projects]([ProjectId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Roles_RoleIdRef')
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Roles_RoleIdRef]
    FOREIGN KEY ([RoleIdRef]) REFERENCES [Roles]([RoleId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_Projects')
    ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Projects]
    FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_UserScopes_Projects_ProjectIdRef')
    ALTER TABLE [UserScopes] ADD CONSTRAINT [FK_UserScopes_Projects_ProjectIdRef]
    FOREIGN KEY ([ProjectIdRef]) REFERENCES [Projects]([ProjectId]);
GO
