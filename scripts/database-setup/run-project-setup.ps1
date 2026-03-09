# =====================================================
# PowerShell Script to Setup Projects and Assign Assets
# =====================================================

# Database connection details
$Server = "192.168.208.26,1433"
$Database = "ITAMS_Shared"
$Username = "itams_user"
$Password = "ITAMS@2024!"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Project Setup and Asset Assignment Tool" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "1. Analyze existing assets" -ForegroundColor Yellow
Write-Host "2. Create projects based on plaza names" -ForegroundColor Yellow
Write-Host "3. Create locations for each project" -ForegroundColor Yellow
Write-Host "4. Assign assets to projects and locations" -ForegroundColor Yellow
Write-Host ""

$confirmation = Read-Host "Do you want to proceed? (Y/N)"
if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "Operation cancelled." -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "Connecting to database..." -ForegroundColor Green

# Build connection string
$ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=true;MultipleActiveResultSets=true"

try {
    # Load SQL script
    $ScriptPath = Join-Path $PSScriptRoot "setup-projects-and-assign-assets.sql"
    
    if (-not (Test-Path $ScriptPath)) {
        Write-Host "ERROR: SQL script not found at: $ScriptPath" -ForegroundColor Red
        exit 1
    }
    
    $SqlScript = Get-Content $ScriptPath -Raw
    
    Write-Host "Executing SQL script..." -ForegroundColor Green
    Write-Host ""
    
    # Create SQL connection
    $Connection = New-Object System.Data.SqlClient.SqlConnection
    $Connection.ConnectionString = $ConnectionString
    $Connection.Open()
    
    # Create SQL command
    $Command = New-Object System.Data.SqlClient.SqlCommand
    $Command.Connection = $Connection
    $Command.CommandText = $SqlScript
    $Command.CommandTimeout = 300 # 5 minutes timeout
    
    # Add info message handler to capture PRINT statements
    $Connection.add_InfoMessage({
        param($sender, $event)
        Write-Host $event.Message -ForegroundColor White
    })
    
    # Execute the script
    $Command.ExecuteNonQuery() | Out-Null
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "Script executed successfully!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    
    # Close connection
    $Connection.Close()
    
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Cyan
    Write-Host "1. Verify the projects in the application" -ForegroundColor White
    Write-Host "2. Assign users to projects" -ForegroundColor White
    Write-Host "3. Test asset access by project" -ForegroundColor White
    
} catch {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "ERROR OCCURRED" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Yellow
    Write-Host $_.Exception.StackTrace -ForegroundColor Yellow
    exit 1
} finally {
    if ($Connection -and $Connection.State -eq 'Open') {
        $Connection.Close()
    }
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
