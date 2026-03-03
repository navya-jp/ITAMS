# Script to add extended fields to all AssetDto mappings in AssetsController.cs

$filePath = "Controllers/AssetsController.cs"
$content = Get-Content $filePath -Raw

# Define the extended fields to add after Department
$extendedFields = @"
                    Classification = a.Classification,
                    OSType = a.OSType,
                    OSVersion = a.OSVersion,
                    DBType = a.DBType,
                    DBVersion = a.DBVersion,
                    IPAddress = a.IPAddress,
                    AssignedUser = a.AssignedUser,
                    UserRole = a.UserRole,
                    ProcuredBy = a.ProcuredBy,
                    PatchStatus = a.PatchStatus,
                    USBBlockingStatus = a.USBBlockingStatus,
                    Remarks = a.Remarks,
"@

# Replace all occurrences of "Department = a.Department," with the extended version
$content = $content -replace "Department = a\.Department,", "Department = a.Department,$extendedFields"

# Also add UpdatedAt to CreatedAt lines
$content = $content -replace "CreatedAt = a\.CreatedAt\s*\}\)", "CreatedAt = a.CreatedAt,`n                    UpdatedAt = a.UpdatedAt`n                })"
$content = $content -replace "CreatedAt = a\.CreatedAt\s*\}\);", "CreatedAt = a.CreatedAt,`n                UpdatedAt = a.UpdatedAt`n            });"

# Save the updated content
Set-Content $filePath $content -NoNewline

Write-Host "Controller mappings updated successfully!"
