# Export Data from Shared Database
# This script exports all data from the shared database to SQL files

param(
    [string]$ServerName = "192.168.208.26,1433",
    [string]$DatabaseName = "ITAMS_Shared",
    [string]$Username = "itams_user",
    [string]$Password = "ITAMS@2024!",
    [string]$OutputFolder = ".\scripts\local-demo-setup\exported-data"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ITAMS Data Export Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create output folder
if (!(Test-Path $OutputFolder)) {
    New-Item -ItemType Directory -Path $OutputFolder | Out-Null
    Write-Host "Created output folder: $OutputFolder" -ForegroundColor Green
}

# Connection string
$connectionString = "Server=$ServerName;Database=$DatabaseName;User Id=$Username;Password=$Password;TrustServerCertificate=True"

Write-Host "Connecting to shared database..." -ForegroundColor Yellow
Write-Host "Server: $ServerName" -ForegroundColor Gray
Write-Host "Database: $DatabaseName" -ForegroundColor Gray
Write-Host ""

# Tables to export in order (respecting foreign keys)
$tables = @(
    @{Name="Roles"; Order=1},
    @{Name="RbacRoles"; Order=2},
    @{Name="RbacPermissions"; Order=3},
    @{Name="RbacRolePermissions"; Order=4},
    @{Name="Projects"; Order=5},
    @{Name="Locations"; Order=6},
    @{Name="Users"; Order=7},
    @{Name="UserProjects"; Order=8},
    @{Name="RbacUserPermissions"; Order=9},
    @{Name="RbacUserScopes"; Order=10},
    @{Name="SystemSettings"; Order=11},
    @{Name="Assets"; Order=12},
    @{Name="AuditEntries"; Order=13},
    @{Name="LoginAudit"; Order=14}
)

$totalTables = $tables.Count
$currentTable = 0

foreach ($table in $tables | Sort-Object Order) {
    $currentTable++
    $tableName = $table.Name
    $outputFile = Join-Path $OutputFolder ("{0:D2}-{1}.sql" -f $table.Order, $tableName)
    
    Write-Host "[$currentTable/$totalTables] Exporting $tableName..." -ForegroundColor Yellow
    
    try {
        # Build BCP command to export data
        $query = "SELECT * FROM [$tableName]"
        
        # Export to CSV first
        $csvFile = Join-Path $OutputFolder "$tableName.csv"
        $bcpCommand = "bcp `"$query`" queryout `"$csvFile`" -S $ServerName -d $DatabaseName -U $Username -P $Password -c -t`",`" -r`"\n`""
        
        Invoke-Expression $bcpCommand | Out-Null
        
        if (Test-Path $csvFile) {
            # Convert CSV to INSERT statements
            $csv = Import-Csv $csvFile -Delimiter ","
            $rowCount = $csv.Count
            
            if ($rowCount -gt 0) {
                # Get column names
                $columns = $csv[0].PSObject.Properties.Name
                $columnList = ($columns | ForEach-Object { "[$_]" }) -join ", "
                
                # Generate INSERT statements
                $insertStatements = @()
                $insertStatements += "-- Exported from $tableName ($rowCount rows)"
                $insertStatements += "-- Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
                $insertStatements += ""
                $insertStatements += "SET IDENTITY_INSERT [$tableName] ON;"
                $insertStatements += ""
                
                foreach ($row in $csv) {
                    $values = @()
                    foreach ($col in $columns) {
                        $value = $row.$col
                        if ([string]::IsNullOrWhiteSpace($value) -or $value -eq "NULL") {
                            $values += "NULL"
                        }
                        elseif ($value -match '^\d+$') {
                            $values += $value
                        }
                        elseif ($value -match '^\d{4}-\d{2}-\d{2}') {
                            $values += "'$value'"
                        }
                        else {
                            $escapedValue = $value.Replace("'", "''")
                            $values += "'$escapedValue'"
                        }
                    }
                    $valueList = $values -join ", "
                    $insertStatements += "INSERT INTO [$tableName] ($columnList) VALUES ($valueList);"
                }
                
                $insertStatements += ""
                $insertStatements += "SET IDENTITY_INSERT [$tableName] OFF;"
                $insertStatements += ""
                
                # Save to SQL file
                $insertStatements | Out-File -FilePath $outputFile -Encoding UTF8
                
                Write-Host "  ✓ Exported $rowCount rows" -ForegroundColor Green
            }
            else {
                Write-Host "  ⚠ Table is empty" -ForegroundColor Yellow
                "-- Table $tableName is empty" | Out-File -FilePath $outputFile -Encoding UTF8
            }
            
            # Clean up CSV
            Remove-Item $csvFile -Force
        }
        else {
            Write-Host "  ✗ Export failed" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  ✗ Error: $_" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Export Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Exported files location:" -ForegroundColor Yellow
Write-Host $OutputFolder -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Run: .\scripts\local-demo-setup\setup-local-database.ps1" -ForegroundColor White
Write-Host "2. Update appsettings.json with local connection string" -ForegroundColor White
Write-Host "3. Run: dotnet run" -ForegroundColor White
Write-Host ""
