# Script to check what columns are required for bulk upload

Write-Host "=== REQUIRED COLUMNS FOR BULK UPLOAD ===" -ForegroundColor Green
Write-Host ""
Write-Host "Your Excel file MUST have these columns:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Asset_Tag       - Required (e.g., ASSET001)" -ForegroundColor Cyan
Write-Host "2. Make            - Required (e.g., Dell, HP)" -ForegroundColor Cyan
Write-Host "3. Model           - Required (e.g., Latitude 5420)" -ForegroundColor Cyan
Write-Host "4. Asset_Type      - Required (e.g., Laptop, Desktop)" -ForegroundColor Cyan
Write-Host "5. Status          - Required (e.g., In Use, Spare, Repair, Decommissioned)" -ForegroundColor Cyan
Write-Host "6. Placing         - Required (e.g., Lane Area, Booth Area, Plaza Area, Server Room, Control Room, Admin Building)" -ForegroundColor Cyan
Write-Host "7. Region          - Required (e.g., North, South)" -ForegroundColor Cyan
Write-Host "8. Location        - Required (e.g., Maharashtra, Delhi)" -ForegroundColor Cyan
Write-Host ""
Write-Host "Optional columns:" -ForegroundColor Yellow
Write-Host "- Serial_Number, Plaza_Name, Department, Sub_Type, Asset_Classification" -ForegroundColor Gray
Write-Host "- OS_Type, OS_Version, DB_Type, DB_Version, IP_Address" -ForegroundColor Gray
Write-Host "- Assigned_User_Name, User_Role, Procured_By, Commissioning_Date" -ForegroundColor Gray
Write-Host "- Criticality, Patch_Status, USB_Blocking_Status, Remarks" -ForegroundColor Gray
Write-Host ""
Write-Host "=== IMPORTANT NOTES ===" -ForegroundColor Green
Write-Host ""
Write-Host "Status values (case-insensitive, flexible):" -ForegroundColor Yellow
Write-Host "  - In Use / inuse / active / working" -ForegroundColor White
Write-Host "  - Spare / spare / available" -ForegroundColor White
Write-Host "  - Repair / repair / maintenance" -ForegroundColor White
Write-Host "  - Decommissioned / decommissioned / retired" -ForegroundColor White
Write-Host ""
Write-Host "Placing values (Title Case):" -ForegroundColor Yellow
Write-Host "  - Lane Area" -ForegroundColor White
Write-Host "  - Booth Area" -ForegroundColor White
Write-Host "  - Plaza Area" -ForegroundColor White
Write-Host "  - Server Room" -ForegroundColor White
Write-Host "  - Control Room" -ForegroundColor White
Write-Host "  - Admin Building" -ForegroundColor White
Write-Host ""
Write-Host "Criticality values (optional):" -ForegroundColor Yellow
Write-Host "  - TMS general" -ForegroundColor White
Write-Host "  - TMS critical" -ForegroundColor White
Write-Host "  - IT general" -ForegroundColor White
Write-Host "  - IT critical" -ForegroundColor White
Write-Host ""
Write-Host "=== NEXT STEPS ===" -ForegroundColor Green
Write-Host ""
Write-Host "1. Open your Excel file" -ForegroundColor White
Write-Host "2. Add a 'Placing' column if it doesn't exist" -ForegroundColor White
Write-Host "3. Fill the Placing column with one of the 6 values above (Title Case)" -ForegroundColor White
Write-Host "4. Make sure Status column has valid values" -ForegroundColor White
Write-Host "5. Save the file and try uploading again" -ForegroundColor White
Write-Host ""
