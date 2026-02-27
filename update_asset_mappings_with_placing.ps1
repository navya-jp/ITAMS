# PowerShell script to update all AssetDto mappings in AssetsController.cs
# Adds Placing field and uses canonical formats for Status and Criticality

$filePath = "Controllers/AssetsController.cs"
$content = Get-Content $filePath -Raw

# Replace all occurrences of the AssetDto mapping
$oldPattern = @'
                    Classification = a\.Classification,
                    OSType = a\.OSType,
                    OSVersion = a\.OSVersion,
                    DBType = a\.DBType,
                    DBVersion = a\.DBVersion,
                    IPAddress = a\.IPAddress,
                    AssignedUserText = a\.AssignedUserText,
                    UserRole = a\.UserRole,
                    ProcuredBy = a\.ProcuredBy,
                    PatchStatus = a\.PatchStatus,
                    USBBlockingStatus = a\.USBBlockingStatus,
                    Remarks = a\.Remarks,
                    UsageCategory = a\.UsageCategory\.ToString\(\),
                    Criticality = a\.Criticality\.ToString\(\),
'@

$newPattern = @'
                    Classification = a.Classification,
                    OSType = a.OSType,
                    OSVersion = a.OSVersion,
                    DBType = a.DBType,
                    DBVersion = a.DBVersion,
                    IPAddress = a.IPAddress,
                    AssignedUserText = a.AssignedUserText,
                    UserRole = a.UserRole,
                    ProcuredBy = a.ProcuredBy,
                    PatchStatus = a.PatchStatus,
                    USBBlockingStatus = a.USBBlockingStatus,
                    Remarks = a.Remarks,
                    UsageCategory = a.UsageCategory.ToString(),
                    Criticality = a.Criticality.ToDisplayString(),
'@

$content = $content -replace $oldPattern, $newPattern

# Replace Status.ToString() with Status.ToCanonicalString()
$content = $content -replace 'Status = a\.Status\.ToString\(\)', 'Status = a.Status.ToCanonicalString()'

# Add Placing field after Status
$statusPattern = 'Status = a\.Status\.ToCanonicalString\(\),'
$statusReplacement = @'
Status = a.Status.ToCanonicalString(),
                    Placing = a.Placing,
'@

$content = $content -replace $statusPattern, $statusReplacement

# Save the updated content
Set-Content -Path $filePath -Value $content

Write-Host "Successfully updated AssetsController.cs with Placing field and canonical formats" -ForegroundColor Green
