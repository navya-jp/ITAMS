-- Migration Phase 2: Generate Sequential IDs
-- Date: 2026-02-10
-- Description: Populates alternate key columns with sequential IDs

PRINT '=====================================================';
PRINT 'PHASE 2: GENERATE SEQUENTIAL IDS';
PRINT '=====================================================';

-- Generate User IDs (USR00001, USR00002, etc.)
UPDATE Users
SET UserId = 'USR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE UserId IS NULL;
PRINT 'Generated User IDs';

-- Generate Role IDs (ROL00001, ROL00002, etc.)
UPDATE Roles
SET RoleId = 'ROL' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RoleId IS NULL;
PRINT 'Generated Role IDs';

-- Generate Permission IDs (PER00001, PER00002, etc.)
UPDATE Permissions
SET PermissionId = 'PER' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE PermissionId IS NULL;
PRINT 'Generated Permission IDs';

-- Generate Project IDs (PRJ00001, PRJ00002, etc.)
UPDATE Projects
SET ProjectId = 'PRJ' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE ProjectId IS NULL;
PRINT 'Generated Project IDs';

-- Generate Location IDs (LOC00001, LOC00002, etc.)
UPDATE Locations
SET LocationId = 'LOC' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE LocationId IS NULL;
PRINT 'Generated Location IDs';

-- Generate Asset IDs (AST00001, AST00002, etc.)
UPDATE Assets
SET AssetId = 'AST' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE AssetId IS NULL;
PRINT 'Generated Asset IDs';

-- Generate RBAC Role IDs (RBR00001, RBR00002, etc.)
UPDATE RbacRoles
SET RbacRoleId = 'RBR' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RbacRoleId IS NULL;
PRINT 'Generated RBAC Role IDs';

-- Generate RBAC Permission IDs (RBP00001, RBP00002, etc.)
UPDATE RbacPermissions
SET RbacPermissionId = 'RBP' + RIGHT('00000' + CAST(Id AS VARCHAR), 5)
WHERE RbacPermissionId IS NULL;
PRINT 'Generated RBAC Permission IDs';

PRINT '';
PRINT 'Phase 2 completed - All sequential IDs generated';
GO
