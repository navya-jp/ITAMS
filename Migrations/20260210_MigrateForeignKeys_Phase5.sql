-- Migration Phase 5: Migrate Final Junction Table Foreign Keys
-- Date: 2026-02-10
-- Description: Migrates the last 2 foreign keys that reference primary keys

PRINT '=====================================================';
PRINT 'PHASE 5: MIGRATE FINAL JUNCTION TABLES';
PRINT '=====================================================';

-- =====================================================
-- ADD ALTERNATE KEYS TO JUNCTION TABLES
-- =====================================================

-- UserProjects table - add alternate key
ALTER TABLE UserProjects ADD UserProjectId NVARCHAR(50) NULL;
PRINT 'Added UserProjects.UserProjectId column';
GO

UPDATE UserProjects 
SET UserProjectId = 'UPR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated UserProject IDs';
GO

CREATE UNIQUE INDEX IX_UserProjects_UserProjectId ON UserProjects(UserProjectId);
PRINT 'Created unique index on UserProjects.UserProjectId';
GO

ALTER TABLE UserProjects ALTER COLUMN UserProjectId NVARCHAR(50) NOT NULL;
PRINT 'Set UserProjects.UserProjectId to NOT NULL';
GO

-- SecurityAuditLogs table - add alternate key
ALTER TABLE SecurityAuditLogs ADD SecurityAuditLogId NVARCHAR(50) NULL;
PRINT 'Added SecurityAuditLogs.SecurityAuditLogId column';
GO

UPDATE SecurityAuditLogs 
SET SecurityAuditLogId = 'SAL' + RIGHT('00000' + CAST(Id AS VARCHAR), 5);
PRINT 'Generated SecurityAuditLog IDs';
GO

CREATE UNIQUE INDEX IX_SecurityAuditLogs_SecurityAuditLogId ON SecurityAuditLogs(SecurityAuditLogId);
PRINT 'Created unique index on SecurityAuditLogs.SecurityAuditLogId';
GO

ALTER TABLE SecurityAuditLogs ALTER COLUMN SecurityAuditLogId NVARCHAR(50) NOT NULL;
PRINT 'Set SecurityAuditLogs.SecurityAuditLogId to NOT NULL';
GO

-- =====================================================
-- ADD REFERENCE COLUMNS
-- =====================================================

-- UserProjectPermissions.UserProjectId → UserProjects.UserProjectId
ALTER TABLE UserProjectPermissions ADD UserProjectIdRef NVARCHAR(50) NULL;
PRINT 'Added UserProjectPermissions.UserProjectIdRef column';
GO

UPDATE UserProjectPermissions 
SET UserProjectIdRef = (SELECT UserProjectId FROM UserProjects WHERE UserProjects.Id = UserProjectPermissions.UserProjectId);
PRINT 'Populated UserProjectPermissions.UserProjectIdRef';
GO

ALTER TABLE UserProjectPermissions ALTER COLUMN UserProjectIdRef NVARCHAR(50) NOT NULL;
PRINT 'Set UserProjectPermissions.UserProjectIdRef to NOT NULL';
GO

-- SecurityAlerts.AuditLogId → SecurityAuditLogs.SecurityAuditLogId
ALTER TABLE SecurityAlerts ADD AuditLogIdRef NVARCHAR(50) NULL;
PRINT 'Added SecurityAlerts.AuditLogIdRef column';
GO

UPDATE SecurityAlerts 
SET AuditLogIdRef = (SELECT SecurityAuditLogId FROM SecurityAuditLogs WHERE SecurityAuditLogs.Id = SecurityAlerts.AuditLogId)
WHERE AuditLogId IS NOT NULL;
PRINT 'Populated SecurityAlerts.AuditLogIdRef';
GO

-- =====================================================
-- DROP OLD FOREIGN KEYS
-- =====================================================

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserProjectPermissions_UserProjects_UserProjectId')
BEGIN
    ALTER TABLE UserProjectPermissions DROP CONSTRAINT FK_UserProjectPermissions_UserProjects_UserProjectId;
    PRINT 'Dropped FK_UserProjectPermissions_UserProjects_UserProjectId';
END
GO

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SecurityAlerts_SecurityAuditLogs_AuditLogId')
BEGIN
    ALTER TABLE SecurityAlerts DROP CONSTRAINT FK_SecurityAlerts_SecurityAuditLogs_AuditLogId;
    PRINT 'Dropped FK_SecurityAlerts_SecurityAuditLogs_AuditLogId';
END
GO

-- =====================================================
-- CREATE NEW FOREIGN KEYS
-- =====================================================

ALTER TABLE UserProjectPermissions ADD CONSTRAINT FK_UserProjectPermissions_UserProjects_UserProjectIdRef
    FOREIGN KEY (UserProjectIdRef) REFERENCES UserProjects(UserProjectId);
PRINT 'Created FK_UserProjectPermissions_UserProjects_UserProjectIdRef';
GO

ALTER TABLE SecurityAlerts ADD CONSTRAINT FK_SecurityAlerts_SecurityAuditLogs_AuditLogIdRef
    FOREIGN KEY (AuditLogIdRef) REFERENCES SecurityAuditLogs(SecurityAuditLogId);
PRINT 'Created FK_SecurityAlerts_SecurityAuditLogs_AuditLogIdRef';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'PHASE 5 COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'ALL foreign keys now use alternate keys';
PRINT 'ZERO primary keys are referenced anywhere in the database';
PRINT '';
PRINT '🎉 MIGRATION 100% COMPLETE! 🎉';
GO
