# 🗄️ ITAMS Database Viewer

Write-Host "=== ITAMS Database Viewer ===" -ForegroundColor Cyan

# Database connection info
$server = ".\SQLEXPRESS"
$database = "ITAMS"

Write-Host "`nDatabase: $database on $server" -ForegroundColor Yellow

# Function to run SQL query and display results
function Run-Query {
    param($query, $title)
    Write-Host "`n=== $title ===" -ForegroundColor Green
    try {
        sqlcmd -S $server -E -d $database -Q $query -W
    } catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Show database tables
Run-Query "SELECT TABLE_NAME as 'Table Name', 
           (SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(TABLE_SCHEMA + '.' + TABLE_NAME)) as 'Columns'
           FROM INFORMATION_SCHEMA.TABLES 
           WHERE TABLE_TYPE = 'BASE TABLE' 
           ORDER BY TABLE_NAME" "Database Tables"

# Show Users with Roles
Run-Query "SELECT u.Id, u.Username, u.Email, u.FirstName + ' ' + u.LastName as 'Full Name', 
           r.Name as 'Role', u.IsActive, u.CreatedAt
           FROM Users u 
           LEFT JOIN Roles r ON u.RoleId = r.Id 
           ORDER BY u.CreatedAt DESC" "Users with Roles"

# Show Roles
Run-Query "SELECT Id, Name, Description, IsSystemRole, IsActive 
           FROM Roles 
           ORDER BY Id" "System Roles"

# Show Projects (if any)
Run-Query "SELECT Id, Name, Code, Description, IsActive, CreatedAt 
           FROM Projects 
           ORDER BY CreatedAt DESC" "Projects"

# Show Locations (if any)
Run-Query "SELECT Id, Name, Region, State, Plaza, IsActive 
           FROM Locations 
           ORDER BY Id" "Locations"

# Show Permissions
Run-Query "SELECT Id, Name, Code, Module, Description 
           FROM Permissions 
           WHERE IsActive = 1 
           ORDER BY Module, Name" "System Permissions"

# Show recent audit entries
Run-Query "SELECT TOP 10 Action, EntityType, UserName, Timestamp 
           FROM AuditEntries 
           ORDER BY Timestamp DESC" "Recent Audit Entries"

Write-Host "`n=== Database Overview Complete ===" -ForegroundColor Cyan
Write-Host "To connect with SQL Server Management Studio:" -ForegroundColor Yellow
Write-Host "Server: .\SQLEXPRESS" -ForegroundColor White
Write-Host "Authentication: Windows Authentication" -ForegroundColor White
Write-Host "Database: ITAMS" -ForegroundColor White