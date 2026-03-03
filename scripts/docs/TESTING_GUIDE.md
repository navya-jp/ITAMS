# Asset Management Refactoring - Testing Guide

## Project Status
✅ **Backend**: Running on http://localhost:5066  
✅ **Frontend**: Running on http://localhost:4200  
✅ **Database**: Connected to shared database at 192.168.208.26,1433

## Quick Test Checklist

### 1. Access the Application
1. Open browser: http://localhost:4200
2. Login with your credentials
3. Navigate to Assets page

### 2. Test Manual Asset Creation

#### Test Case 1: Valid Asset with New Fields
1. Click "Create Asset" button
2. Fill in Tab 1 (Basic Details):
   - Location: Select any location
   - Asset Tag: TEST001
   - Usage Category: TMS
   - Criticality: Select "TMS general" (new format!)
   - Status: Select "inuse" (lowercase!)
   - **Placing**: Select "server room" (NEW FIELD!)
3. Click "Next" to Tab 2
4. Fill in Asset Details:
   - Asset Type: Hardware
   - Make: Dell
   - Model: PowerEdge R740
5. Click "Next" to Tab 3 (optional)
6. Click "Create Asset"
7. ✅ **Expected**: Asset created successfully

#### Test Case 2: Missing Placing Field
1. Click "Create Asset"
2. Fill in all fields EXCEPT Placing
3. Try to proceed to next tab
4. ✅ **Expected**: Validation error "Placing is required"

#### Test Case 3: View Asset Details
1. Click "View" (eye icon) on any asset
2. Scroll to Location Information section
3. ✅ **Expected**: See "Placing" field displayed

### 3. Test Bulk Upload

#### Test Case 1: Download Template
1. Click "Bulk Upload" button
2. Click "Download Template" link
3. Open the Excel file
4. ✅ **Expected**: See new columns:
   - Status (with sample "inuse")
   - Criticality (with sample "IT general")
   - Placing (with sample "server room")

#### Test Case 2: Valid Bulk Upload
1. Create Excel file with these columns:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Criticality | Placing | Region | Location
   TEST-BLK-001 | HP | ProLiant | Server | inuse | IT general | server room | North | Maharashtra
   TEST-BLK-002 | Dell | Latitude | Laptop | spare | TMS critical | admin building | South | Karnataka
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Success message: "Successfully uploaded 2 assets!"
   - Both assets appear in the list

#### Test Case 3: Invalid Status
1. Create Excel with invalid status:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Placing | Region | Location
   TEST-INV-001 | HP | ProLiant | Server | unknown | server room | North | Maharashtra
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Error message showing failed row
   - Error detail: "Invalid Status: Invalid status value: 'unknown'. Expected: inuse, spare, repair, or decommissioned"

#### Test Case 4: Invalid Placing
1. Create Excel with invalid placing:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Placing | Region | Location
   TEST-INV-002 | HP | ProLiant | Server | inuse | parking lot | North | Maharashtra
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Error message showing failed row
   - Error detail: "Invalid Placing: Invalid placing value: 'parking lot'. Expected one of: lane area, booth area, plaza area, server room, control room, admin building"

#### Test Case 5: Missing Required Fields
1. Create Excel missing Region or Location:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Placing
   TEST-INV-003 | HP | ProLiant | Server | inuse | server room
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Error: "Region is required" or "Location (state or district name) is required"

#### Test Case 6: Partial Success
1. Create Excel with mix of valid and invalid rows:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Placing | Region | Location
   TEST-MIX-001 | HP | ProLiant | Server | inuse | server room | North | Maharashtra
   TEST-MIX-002 | Dell | Latitude | Laptop | invalid | admin building | South | Karnataka
   TEST-MIX-003 | Cisco | Catalyst | Switch | spare | control room | East | West Bengal
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Success count: 2
   - Failed count: 1
   - Error details for row 2 only
   - Valid rows (1 and 3) are imported

#### Test Case 7: Legacy Status Format (Backward Compatibility)
1. Create Excel with old status format:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Placing | Region | Location
   TEST-LEG-001 | HP | ProLiant | Server | InUse | server room | North | Maharashtra
   TEST-LEG-002 | Dell | Latitude | Laptop | Spare | admin building | South | Karnataka
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Both rows imported successfully
   - Status normalized to lowercase ("inuse", "spare")

#### Test Case 8: Typo "decommitioned"
1. Create Excel with typo:
   ```
   Asset_Tag | Make | Model | Asset_Type | Status | Placing | Region | Location
   TEST-TYPO-001 | HP | ProLiant | Server | decommitioned | server room | North | Maharashtra
   ```
2. Upload the file
3. ✅ **Expected**: 
   - Row imported successfully
   - Status corrected to "decommissioned"

### 4. Test Edit Asset
1. Click "Edit" (pencil icon) on any asset
2. Change Placing to different value
3. Change Status to different value
4. Click "Save Changes"
5. ✅ **Expected**: Asset updated successfully

### 5. Verify API Responses

#### Check Status Format
1. Open browser DevTools (F12)
2. Go to Network tab
3. Refresh Assets page
4. Click on the API call to `/api/assets`
5. Check Response
6. ✅ **Expected**: 
   - Status values are lowercase: "inuse", "spare", "repair", "decommissioned"
   - Criticality values are display format: "TMS general", "TMS critical", "IT general", "IT critical"
   - Placing values are present

## Dropdown Values Reference

### Status (Canonical - Lowercase)
- inuse
- spare
- repair
- decommissioned

### Criticality (Display Format)
- TMS general
- TMS critical
- IT general
- IT critical

### Placing (Exact Values)
- lane area
- booth area
- plaza area
- server room
- control room
- admin building

## Common Issues & Solutions

### Issue: "Placing is required" error
**Solution**: Ensure Placing dropdown is selected in create form

### Issue: Bulk upload fails with status error
**Solution**: Use lowercase status values: "inuse", "spare", "repair", "decommissioned"

### Issue: Criticality not showing correctly
**Solution**: Use display format: "TMS general", "TMS critical", "IT general", "IT critical"

### Issue: Location validation error
**Solution**: Ensure both Region and Location (state/district) columns have values

### Issue: Old Excel files fail
**Solution**: Add new required columns: Criticality and Placing

## Success Indicators

✅ Create form shows Placing dropdown with 6 options  
✅ Status dropdown shows lowercase values  
✅ Criticality dropdown shows display format  
✅ Placing validation works (required field)  
✅ Bulk upload template has new columns  
✅ Invalid status returns specific error message  
✅ Invalid placing returns specific error message  
✅ Partial success works (some valid, some invalid)  
✅ Legacy formats are accepted and normalized  
✅ API responses use canonical formats  
✅ View modal displays Placing field  
✅ Edit form includes Placing dropdown  
✅ Status badges work with lowercase values  

## Next Steps After Testing

1. If all tests pass:
   - Document any edge cases discovered
   - Update user documentation
   - Train users on new fields
   - Monitor production for issues

2. If tests fail:
   - Check browser console for errors
   - Check backend logs for exceptions
   - Verify database has Placing column
   - Ensure both backend and frontend are latest version

## Support

If you encounter issues:
1. Check browser console (F12)
2. Check backend logs in terminal
3. Verify database schema has Placing column
4. Ensure all files are saved and processes restarted
5. Clear browser cache and reload

## Database Verification

To verify Placing column exists:
```sql
SELECT TOP 1 * FROM Assets
```

Expected columns should include:
- Placing (nvarchar(50), NOT NULL)
- Status (int, NOT NULL)
- Criticality (int, NOT NULL)
