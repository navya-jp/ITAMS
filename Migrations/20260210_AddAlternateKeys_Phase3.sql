-- Migration Phase 3: Make Alternate Keys NOT NULL
-- Date: 2026-02-10
-- Description: Sets alternate key columns to NOT NULL after populating them

PRINT '=====================================================';
PRINT 'PHASE 3: MAKE ALTERNATE KEYS NOT NULL';
PRINT '=====================================================';

ALTER TABLE Users ALTER COLUMN UserId NVARCHAR(50) NOT NULL;
PRINT 'Users.UserId set to NOT NULL';

ALTER TABLE Roles ALTER COLUMN RoleId NVARCHAR(50) NOT NULL;
PRINT 'Roles.RoleId set to NOT NULL';

ALTER TABLE Permissions ALTER COLUMN PermissionId NVARCHAR(50) NOT NULL;
PRINT 'Permissions.PermissionId set to NOT NULL';

ALTER TABLE Projects ALTER COLUMN ProjectId NVARCHAR(50) NOT NULL;
PRINT 'Projects.ProjectId set to NOT NULL';

ALTER TABLE Locations ALTER COLUMN LocationId NVARCHAR(50) NOT NULL;
PRINT 'Locations.LocationId set to NOT NULL';

ALTER TABLE Assets ALTER COLUMN AssetId NVARCHAR(50) NOT NULL;
PRINT 'Assets.AssetId set to NOT NULL';

ALTER TABLE RbacRoles ALTER COLUMN RbacRoleId NVARCHAR(50) NOT NULL;
PRINT 'RbacRoles.RbacRoleId set to NOT NULL';

ALTER TABLE RbacPermissions ALTER COLUMN RbacPermissionId NVARCHAR(50) NOT NULL;
PRINT 'RbacPermissions.RbacPermissionId set to NOT NULL';

PRINT '';
PRINT '=====================================================';
PRINT 'ALL PHASES COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'All tables now have alternate key columns with sequential IDs';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Update Entity Framework models to include alternate keys';
PRINT '2. Update DTOs to use alternate keys';
PRINT '3. Update controllers and services to expose alternate keys';
GO
