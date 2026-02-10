$connectionString = "Server=192.168.208.10,1433;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=true"

$query = @"
-- First, revoke all existing Project Manager permissions
UPDATE RbacRolePermissions 
SET Status = 'REVOKED', RevokedAt = GETUTCDATE(), RevokedBy = 1
WHERE RoleId = 5 AND Status = 'ACTIVE';

-- USER_MANAGEMENT: USER_VIEW, USER_EDIT
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'USER_MANAGEMENT' AND Code IN ('USER_VIEW', 'USER_EDIT') AND Status = 'ACTIVE'
AND NOT EXISTS (SELECT 1 FROM RbacRolePermissions WHERE RoleId = 5 AND PermissionId = RbacPermissions.Id);

UPDATE RbacRolePermissions 
SET Status = 'ACTIVE', GrantedAt = GETUTCDATE(), GrantedBy = 1, RevokedAt = NULL, RevokedBy = NULL
WHERE RoleId = 5 AND PermissionId IN (SELECT Id FROM RbacPermissions WHERE Module = 'USER_MANAGEMENT' AND Code IN ('USER_VIEW', 'USER_EDIT'));

-- ASSET_MANAGEMENT: ASSET_VIEW, ASSET_EDIT, ASSET_CREATE
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'ASSET_MANAGEMENT' AND Code IN ('ASSET_VIEW', 'ASSET_EDIT', 'ASSET_CREATE') AND Status = 'ACTIVE'
AND NOT EXISTS (SELECT 1 FROM RbacRolePermissions WHERE RoleId = 5 AND PermissionId = RbacPermissions.Id);

UPDATE RbacRolePermissions 
SET Status = 'ACTIVE', GrantedAt = GETUTCDATE(), GrantedBy = 1, RevokedAt = NULL, RevokedBy = NULL
WHERE RoleId = 5 AND PermissionId IN (SELECT Id FROM RbacPermissions WHERE Module = 'ASSET_MANAGEMENT' AND Code IN ('ASSET_VIEW', 'ASSET_EDIT', 'ASSET_CREATE'));

-- LIFECYCLE_REPAIRS: LIFECYCLE_VIEW, REPAIR_VIEW, REPAIR_ADD, MAINTENANCE_SCHEDULE
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'LIFECYCLE_REPAIRS' AND Code IN ('LIFECYCLE_VIEW', 'REPAIR_VIEW', 'REPAIR_ADD', 'MAINTENANCE_SCHEDULE') AND Status = 'ACTIVE'
AND NOT EXISTS (SELECT 1 FROM RbacRolePermissions WHERE RoleId = 5 AND PermissionId = RbacPermissions.Id);

UPDATE RbacRolePermissions 
SET Status = 'ACTIVE', GrantedAt = GETUTCDATE(), GrantedBy = 1, RevokedAt = NULL, RevokedBy = NULL
WHERE RoleId = 5 AND PermissionId IN (SELECT Id FROM RbacPermissions WHERE Module = 'LIFECYCLE_REPAIRS' AND Code IN ('LIFECYCLE_VIEW', 'REPAIR_VIEW', 'REPAIR_ADD', 'MAINTENANCE_SCHEDULE'));

-- REPORTS_AUDITS: All permissions
INSERT INTO RbacRolePermissions (RoleId, PermissionId, Allowed, GrantedAt, GrantedBy, Status)
SELECT 5, Id, 1, GETUTCDATE(), 1, 'ACTIVE'
FROM RbacPermissions
WHERE Module = 'REPORTS_AUDITS' AND Status = 'ACTIVE'
AND NOT EXISTS (SELECT 1 FROM RbacRolePermissions WHERE RoleId = 5 AND PermissionId = RbacPermissions.Id);

UPDATE RbacRolePermissions 
SET Status = 'ACTIVE', GrantedAt = GETUTCDATE(), GrantedBy = 1, RevokedAt = NULL, RevokedBy = NULL
WHERE RoleId = 5 AND PermissionId IN (SELECT Id FROM RbacPermissions WHERE Module = 'REPORTS_AUDITS');
"@

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Fixing Project Manager permissions..." -ForegroundColor Cyan
    
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $command.ExecuteNonQuery() | Out-Null
    
    Write-Host "Project Manager permissions fixed!" -ForegroundColor Green
    
    # Verify
    $verifyQuery = "SELECT COUNT(*) FROM RbacRolePermissions WHERE RoleId = 5 AND Status = 'ACTIVE'"
    $verifyCommand = $connection.CreateCommand()
    $verifyCommand.CommandText = $verifyQuery
    $count = $verifyCommand.ExecuteScalar()
    
    Write-Host "Project Manager now has $count permissions" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
