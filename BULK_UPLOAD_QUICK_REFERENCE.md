# Bulk Upload - Quick Reference Card

## ✅ What You Need

### Required Columns (any variation)
- ✓ Asset Tag
- ✓ Asset Type  
- ✓ Make
- ✓ Model
- ✓ Status

### Valid Status Values
- InUse
- Spare
- Repair
- Decommissioned

## 🎯 How It Works

1. **Your Excel file can have ANY column order**
2. **Column names can vary** (e.g., "Tag" or "Asset_Tag" or "Asset Tag")
3. **System auto-detects** your column structure
4. **No reformatting needed** - use your existing files!

## 📋 Common Column Name Variations

| What You Call It | System Recognizes |
|-----------------|-------------------|
| Tag, Asset Tag, Asset_Tag, AssetTag, Asset ID | ✅ All work |
| SN, Serial No, Serial Number, Serial_Number | ✅ All work |
| Type, Asset Type, Asset_Type, AssetType | ✅ All work |
| Make, Manufacturer, Brand | ✅ All work |
| Site, Plaza, Site Name, Plaza Name | ✅ All work |
| IP, IP Address, IP_Address, IPAddress | ✅ All work |
| Notes, Comments, Remarks, Description | ✅ All work |

## 🚀 Quick Steps

```
1. Open your Excel file
2. Make sure you have the 5 required columns
3. Go to "Bulk Upload" in the menu
4. Upload your file
5. Done!
```

## ⚠️ Common Mistakes

| Error | Fix |
|-------|-----|
| Missing required columns | Add Asset Tag, Type, Make, Model, Status |
| Duplicate Asset Tag | Each tag must be unique |
| Invalid Status | Use only: InUse, Spare, Repair, Decommissioned |
| Bad date format | Use: 2024-01-15 or 01/15/2024 |
| Bad IP address | Use: 192.168.1.1 format |

## 💡 Pro Tips

- Start with 5-10 rows to test
- Check for duplicate Asset Tags before uploading
- Keep a backup of your original file
- Review error messages for specific row numbers
- Use consistent date format throughout

## 📊 Example Formats That Work

### Format 1
```
Asset_Tag | Serial_Number | Asset_Type | Make | Model | Status
```

### Format 2
```
Tag | SN | Type | Manufacturer | Model Number | Asset Status
```

### Format 3
```
Make | Model | Tag | Type | Status
```

### Format 4 (Different Order)
```
Status | Model | Make | Type | Tag | IP Address | Site Name
```

**All formats work! 🎉**

## 📁 File Requirements

- Format: .xlsx only
- Max Size: 50MB
- Max Rows: ~10,000 recommended

## 🆘 Need Help?

1. Download the sample template
2. Check error messages for row numbers
3. Verify column names match variations above
4. Contact admin if issues persist

---

**Remember**: Your Excel file, your way! The system adapts to your format. 🚀
