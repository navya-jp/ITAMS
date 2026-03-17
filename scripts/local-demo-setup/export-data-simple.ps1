# Simple Data Export using sqlcmd
# Exports all data from shared database to a single SQL file

param(
    [string]$SharedServer = "192.168.208.26,1433",
    [string]$SharedDatabase = "ITAMS_Shared",
    [string]$Username = "itams_user",
    [string]$Password = "ITAMS@2024!",
    [string]$OutputFile = ".\scripts\local-demo-setup\ITAMS_Data_Export.sql"
)

Write-Host "Exporting data from shared database..." -ForegroundColor Cyan
Write-Host "This may take a few minutes..." -ForegroundColor Yellow
Write-Host ""

# Export script that generates INSERT statements
$exportScript = @"
USE [$SharedDatabase];
GO

-- Disable constraints for import
PRINT 'SET NOCOUNT ON;';
PRINT 'GO';
PRINT '';

-- Export Roles
PRINT '-- ========================================';
PRINT '-- Roles';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [Roles] ON;';
SELECT 'INSERT INTO [Roles] ([Id],[RoleId],[Name],[Description],[IsSystemRole],[IsActive],[CreatedAt]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([RoleId], '''') + ',' +
    QUOTENAME([Name], '''') + ',' +
    ISNULL(QUOTENAME([Description], ''''), 'NULL') + ',' +
    CAST([IsSystemRole] AS VARCHAR) + ',' +
    CAST([IsActive] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ');'
FROM [Roles];
PRINT 'SET IDENTITY_INSERT [Roles] OFF;';
PRINT 'GO';
PRINT '';

-- Export RBAC Roles
PRINT '-- ========================================';
PRINT '-- RBAC Roles';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [RbacRoles] ON;';
SELECT 'INSERT INTO [RbacRoles] ([Id],[RoleId],[Name],[Description],[IsSystemRole],[IsActive],[CreatedAt],[CreatedBy],[UpdatedAt],[UpdatedBy]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([RoleId], '''') + ',' +
    QUOTENAME([Name], '''') + ',' +
    ISNULL(QUOTENAME([Description], ''''), 'NULL') + ',' +
    CAST([IsSystemRole] AS VARCHAR) + ',' +
    CAST([IsActive] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ',' +
    ISNULL(CAST([CreatedBy] AS VARCHAR), 'NULL') + ',' +
    ISNULL(QUOTENAME(CONVERT(VARCHAR, [UpdatedAt], 120), ''''), 'NULL') + ',' +
    ISNULL(CAST([UpdatedBy] AS VARCHAR), 'NULL') + ');'
FROM [RbacRoles];
PRINT 'SET IDENTITY_INSERT [RbacRoles] OFF;';
PRINT 'GO';
PRINT '';

-- Export RBAC Permissions
PRINT '-- ========================================';
PRINT '-- RBAC Permissions';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [RbacPermissions] ON;';
SELECT 'INSERT INTO [RbacPermissions] ([Id],[PermissionId],[Name],[Code],[Description],[Module],[IsActive],[CreatedAt],[CreatedBy]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([PermissionId], '''') + ',' +
    QUOTENAME([Name], '''') + ',' +
    QUOTENAME([Code], '''') + ',' +
    ISNULL(QUOTENAME([Description], ''''), 'NULL') + ',' +
    QUOTENAME([Module], '''') + ',' +
    CAST([IsActive] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ',' +
    ISNULL(CAST([CreatedBy] AS VARCHAR), 'NULL') + ');'
FROM [RbacPermissions];
PRINT 'SET IDENTITY_INSERT [RbacPermissions] OFF;';
PRINT 'GO';
PRINT '';

-- Export Projects
PRINT '-- ========================================';
PRINT '-- Projects';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [Projects] ON;';
SELECT 'INSERT INTO [Projects] ([Id],[ProjectId],[Name],[PreferredName],[Description],[Code],[SPVName],[States],[IsActive],[CreatedAt],[CreatedBy]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([ProjectId], '''') + ',' +
    QUOTENAME([Name], '''') + ',' +
    ISNULL(QUOTENAME([PreferredName], ''''), 'NULL') + ',' +
    ISNULL(QUOTENAME([Description], ''''), 'NULL') + ',' +
    QUOTENAME([Code], '''') + ',' +
    QUOTENAME([SPVName], '''') + ',' +
    QUOTENAME([States], '''') + ',' +
    CAST([IsActive] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ',' +
    ISNULL(CAST([CreatedBy] AS VARCHAR), 'NULL') + ');'
FROM [Projects];
PRINT 'SET IDENTITY_INSERT [Projects] OFF;';
PRINT 'GO';
PRINT '';

-- Export Locations
PRINT '-- ========================================';
PRINT '-- Locations';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [Locations] ON;';
SELECT 'INSERT INTO [Locations] ([Id],[LocationId],[Name],[Type],[ParentLocationId],[ProjectId],[IsActive],[CreatedAt],[CreatedBy]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([LocationId], '''') + ',' +
    QUOTENAME([Name], '''') + ',' +
    QUOTENAME([Type], '''') + ',' +
    ISNULL(CAST([ParentLocationId] AS VARCHAR), 'NULL') + ',' +
    ISNULL(CAST([ProjectId] AS VARCHAR), 'NULL') + ',' +
    CAST([IsActive] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ',' +
    ISNULL(CAST([CreatedBy] AS VARCHAR), 'NULL') + ');'
FROM [Locations];
PRINT 'SET IDENTITY_INSERT [Locations] OFF;';
PRINT 'GO';
PRINT '';

-- Export Users (with password hashes)
PRINT '-- ========================================';
PRINT '-- Users';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [Users] ON;';
SELECT 'INSERT INTO [Users] ([Id],[UserId],[Username],[Email],[FirstName],[LastName],[PasswordHash],[RoleId],[IsActive],[MustChangePassword],[CreatedAt],[ProjectId]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([UserId], '''') + ',' +
    QUOTENAME([Username], '''') + ',' +
    QUOTENAME([Email], '''') + ',' +
    QUOTENAME([FirstName], '''') + ',' +
    QUOTENAME([LastName], '''') + ',' +
    QUOTENAME([PasswordHash], '''') + ',' +
    CAST([RoleId] AS VARCHAR) + ',' +
    CAST([IsActive] AS VARCHAR) + ',' +
    CAST([MustChangePassword] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ',' +
    ISNULL(CAST([ProjectId] AS VARCHAR), 'NULL') + ');'
FROM [Users];
PRINT 'SET IDENTITY_INSERT [Users] OFF;';
PRINT 'GO';
PRINT '';

-- Export System Settings
PRINT '-- ========================================';
PRINT '-- System Settings';
PRINT '-- ========================================';
PRINT 'SET IDENTITY_INSERT [SystemSettings] ON;';
SELECT 'INSERT INTO [SystemSettings] ([Id],[SettingKey],[SettingValue],[Category],[Description],[DataType],[IsEditable],[CreatedAt],[UpdatedAt],[UpdatedBy]) VALUES (' +
    CAST([Id] AS VARCHAR) + ',' +
    QUOTENAME([SettingKey], '''') + ',' +
    QUOTENAME([SettingValue], '''') + ',' +
    QUOTENAME([Category], '''') + ',' +
    ISNULL(QUOTENAME([Description], ''''), 'NULL') + ',' +
    QUOTENAME([DataType], '''') + ',' +
    CAST([IsEditable] AS VARCHAR) + ',' +
    QUOTENAME(CONVERT(VARCHAR, [CreatedAt], 120), '''') + ',' +
    ISNULL(QUOTENAME(CONVERT(VARCHAR, [UpdatedAt], 120), ''''), 'NULL') + ',' +
    ISNULL(CAST([UpdatedBy] AS VARCHAR), 'NULL') + ');'
FROM [SystemSettings];
PRINT 'SET IDENTITY_INSERT [SystemSettings] OFF;';
PRINT 'GO';
PRINT '';

PRINT '-- Export completed successfully';
"@

# Run export
try {
    $exportScript | sqlcmd -S $SharedServer -U $Username -P $Password -d $SharedDatabase -h -1 -W | Out-File -FilePath $OutputFile -Encoding UTF8
    
    Write-Host "✓ Data exported successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output file: $OutputFile" -ForegroundColor Cyan
    Write-Host "File size: $((Get-Item $OutputFile).Length / 1KB) KB" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Next step: Run setup-local-database.ps1" -ForegroundColor Yellow
}
catch {
    Write-Host "✗ Export failed: $_" -ForegroundColor Red
    exit 1
}
