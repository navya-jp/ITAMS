-- Migration: Remove SessionId from Audit Logs
-- Date: 2026-02-18
-- Description: Removes SessionId column from LoginAudit, rbac_access_audit_log, and rbac_permission_audit_log tables

USE ITAMS_Shared;
GO

PRINT 'Starting SessionId removal from audit logs...';
GO

-- Remove SessionId from LoginAudit table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('LoginAudit') AND name = 'SessionId')
BEGIN
    PRINT 'Removing SessionId column from LoginAudit table...';
    ALTER TABLE LoginAudit DROP COLUMN SessionId;
    PRINT 'SessionId column removed from LoginAudit table.';
END
ELSE
BEGIN
    PRINT 'SessionId column does not exist in LoginAudit table.';
END
GO

-- Remove session_id from rbac_access_audit_log table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('rbac_access_audit_log') AND name = 'session_id')
BEGIN
    PRINT 'Removing session_id column from rbac_access_audit_log table...';
    ALTER TABLE rbac_access_audit_log DROP COLUMN session_id;
    PRINT 'session_id column removed from rbac_access_audit_log table.';
END
ELSE
BEGIN
    PRINT 'session_id column does not exist in rbac_access_audit_log table.';
END
GO

-- Remove session_id from rbac_permission_audit_log table
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('rbac_permission_audit_log') AND name = 'session_id')
BEGIN
    PRINT 'Removing session_id column from rbac_permission_audit_log table...';
    ALTER TABLE rbac_permission_audit_log DROP COLUMN session_id;
    PRINT 'session_id column removed from rbac_permission_audit_log table.';
END
ELSE
BEGIN
    PRINT 'session_id column does not exist in rbac_permission_audit_log table.';
END
GO

PRINT 'SessionId removal from audit logs completed successfully.';
GO
