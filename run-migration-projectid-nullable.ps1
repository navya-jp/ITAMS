$connectionString = "Server=(local)\SQLEXPRESS;Database=ITAMS_Shared;User Id=itams_user;Password=ITAMS@2024!;TrustServerCertificate=True"

$migrationFile = "Migrations/20260223_MakeUserProjectIdNullable.sql"

Write-Host "Running migration: $migrationFile" -ForegroundColor Cyan

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $sql = Get-Content $migrationFile -Raw
    
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    $command.ExecuteNonQuery() | Out-Null
    
    Write-Host "Migration completed successfully!" -ForegroundColor Green
    
    $connection.Close()
}
catch {
    Write-Host "Error running migration: $_" -ForegroundColor Red
}
