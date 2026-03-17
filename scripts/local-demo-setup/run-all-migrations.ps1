# Run All Migrations on Local Database

Write-Host "Running all migrations on local database..." -ForegroundColor Cyan
Write-Host ""

$localServer = ".\SQLEXPRESS"
$localDb = "ITAMS_Local"

# Essential migrations in order
$migrations = @(
    "20260209_AddAuditColumnsToAllTables.sql",
    "20260210_AddProjectFields.sql",
    "20260213_AddLastActivityAt.sql",
    "20260213_AddProjectLocationAccessControl.sql",
    "20260213_AddSessionManagement.sql",
    "20260213_CreateLoginAuditTable.sql",
    "20260223_CreateApprovalWorkflowTables.sql",
    "20260223_CreateAssetMasterFieldsTables.sql",
    "20260223_CreateMasterDataTables.sql",
    "20260223_MakeUserProjectIdNullable.sql",
    "20260225_AddAssetExtendedFields.sql",
    "20260225_AddLocationTextFields.sql",
    "20260227_AddPlacingFieldAndUpdateEnums.sql"
)

$i = 0
foreach ($migration in $migrations) {
    $i++
    $migrationPath = ".\Migrations\$migration"
    
    if (Test-Path $migrationPath) {
        Write-Host "[$i/$($migrations.Count)] Running $migration..." -ForegroundColor Yellow
        
        try {
            sqlcmd -S $localServer -d $localDb -i $migrationPath 2>&1 | Out-Null
            Write-Host "  Done" -ForegroundColor Green
        }
        catch {
            Write-Host "  Warning: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "[$i/$($migrations.Count)] Skipping $migration (not found)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "All migrations completed!" -ForegroundColor Green
Write-Host ""

# Show table list
Write-Host "Tables in local database:" -ForegroundColor Cyan
sqlcmd -S $localServer -d $localDb -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
