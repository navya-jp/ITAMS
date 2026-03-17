# Copy ITAMS Project (Clean - No Build Artifacts)
# This script copies the project excluding unnecessary files for transfer/demo

param(
    [string]$Destination = "C:\Users\$env:USERNAME\Desktop\ITAMS-Clean"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "ITAMS Project Clean Copy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$Source = $PSScriptRoot | Split-Path -Parent
Write-Host "Source: $Source" -ForegroundColor Yellow
Write-Host "Destination: $Destination" -ForegroundColor Yellow
Write-Host ""

# Create destination directory
if (Test-Path $Destination) {
    Write-Host "Destination already exists. Removing..." -ForegroundColor Yellow
    Remove-Item -Path $Destination -Recurse -Force
}
New-Item -ItemType Directory -Path $Destination -Force | Out-Null

Write-Host "Copying project files (excluding build artifacts)..." -ForegroundColor Green

# Folders to exclude
$ExcludeFolders = @(
    'node_modules',
    'bin',
    'obj',
    '.angular',
    '.git',
    '.vs',
    '.vscode',
    'logs'
)

# File patterns to exclude
$ExcludeFiles = @(
    '*.log',
    '*.db',
    '*.db-shm',
    '*.db-wal',
    '*.user',
    '*.suo',
    '*.cache',
    '*_perms.txt',
    '*_data.csv',
    'role_perm_*.txt',
    'shared_rbac_*.txt',
    'admin_perms.txt',
    'pm_perms.txt',
    'auditor_perms.txt',
    'itstaff_perms.txt'
)

# Copy function with exclusions
function Copy-ProjectFiles {
    param($SourcePath, $DestPath)
    
    Get-ChildItem -Path $SourcePath -Recurse | ForEach-Object {
        $relativePath = $_.FullName.Substring($SourcePath.Length)
        $destFile = Join-Path $DestPath $relativePath
        
        # Check if it's an excluded folder
        $isExcluded = $false
        foreach ($excludeFolder in $ExcludeFolders) {
            if ($relativePath -like "*\$excludeFolder\*" -or $relativePath -like "*\$excludeFolder") {
                $isExcluded = $true
                break
            }
        }
        
        # Check if it's an excluded file pattern
        if (-not $isExcluded) {
            foreach ($pattern in $ExcludeFiles) {
                if ($_.Name -like $pattern) {
                    $isExcluded = $true
                    break
                }
            }
        }
        
        if (-not $isExcluded) {
            if ($_.PSIsContainer) {
                # Create directory
                if (-not (Test-Path $destFile)) {
                    New-Item -ItemType Directory -Path $destFile -Force | Out-Null
                }
            } else {
                # Copy file
                $destDir = Split-Path $destFile -Parent
                if (-not (Test-Path $destDir)) {
                    New-Item -ItemType Directory -Path $destDir -Force | Out-Null
                }
                Copy-Item -Path $_.FullName -Destination $destFile -Force
            }
        }
    }
}

# Perform the copy
Copy-ProjectFiles -SourcePath $Source -DestPath $Destination

Write-Host ""
Write-Host "✓ Project copied successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Excluded:" -ForegroundColor Yellow
Write-Host "  - Build artifacts (bin, obj)" -ForegroundColor Gray
Write-Host "  - Node modules (run 'npm install' in itams-frontend)" -ForegroundColor Gray
Write-Host "  - Git history (.git)" -ForegroundColor Gray
Write-Host "  - IDE files (.vs, .vscode)" -ForegroundColor Gray
Write-Host "  - Log files" -ForegroundColor Gray
Write-Host "  - Database files (*.db)" -ForegroundColor Gray
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Open $Destination\ITAMS.slnx in Visual Studio" -ForegroundColor White
Write-Host "  2. Run 'npm install' in itams-frontend folder" -ForegroundColor White
Write-Host "  3. Update appsettings.json with your database connection" -ForegroundColor White
Write-Host ""
Write-Host "Location: $Destination" -ForegroundColor Green
