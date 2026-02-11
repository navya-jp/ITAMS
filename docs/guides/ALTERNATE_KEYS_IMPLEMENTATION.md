# Alternate Keys Implementation Guide

## Overview
This document describes the implementation of alternate keys (sequential business IDs) to replace primary key references in the ITAMS database.

## Design Pattern

### Before (Using Primary Keys)
```
Users: Id=1 (PK)
Projects: CreatedBy=1 (FK → Users.Id)
```

### After (Using Alternate Keys)
```
Users: Id=1 (PK, internal), UserId='USR00001' (Alternate Key)
Projects: CreatedBy='USR00001' (FK → Users.UserId)
```

## Alternate Key Format

| Table | Prefix | Format | Example |
|-------|--------|--------|---------|
| Users | USR | USR00001 | USR00001, USR00002 |
| Roles | ROL | ROL00001 | ROL00001, ROL00002 |
| Permissions | PER | PER00001 | PER00001, PER00002 |
| Projects | PRJ | PRJ00001 | PRJ00001, PRJ00002 |
| Locations | LOC | LOC00001 | LOC00001, LOC00002 |
| Assets | AST | AST00001 | AST00001, AST00002 |

## Benefits

1. ✅ **Primary keys remain internal** - Never exposed in relationships
2. ✅ **Business-friendly identifiers** - Meaningful and readable
3. ✅ **Referential integrity maintained** - Database constraints still enforced
4. ✅ **Flexibility** - Can change internal IDs without affecting relationships
5. ✅ **Better for APIs** - Expose business IDs instead of internal IDs

## Migration Steps

### Step 1: Add Alternate Key Columns
```sql
ALTER TABLE Users ADD UserId NVARCHAR(50) UNIQUE NOT NULL;
ALTER TABLE Projects ADD ProjectId NVARCHAR(50) UNIQUE NOT NULL;
-- etc.
```

### Step 2: Generate Sequential IDs for Existing Data
```sql
UPDATE Users SET UserId = 'USR' + RIGHT('00000' + CAST(ROW_NUMBER()...
```

### Step 3: Add Reference Columns
```sql
ALTER TABLE Projects ADD CreatedByUserIdRef NVARCHAR(50);
UPDATE Projects SET CreatedByUserIdRef = (SELECT UserId FROM Users WHERE Users.Id = Projects.CreatedBy);
```

### Step 4: Drop Old Foreign Keys
```sql
ALTER TABLE Projects DROP CONSTRAINT FK_Projects_Users_CreatedBy;
```

### Step 5: Create New Foreign Keys
```sql
ALTER TABLE Projects ADD CONSTRAINT FK_Projects_Users_CreatedByUserIdRef 
    FOREIGN KEY (CreatedByUserIdRef) REFERENCES Users(UserId);
```

### Step 6: Update Application Code
- Update Entity Framework models
- Update DTOs
- Update Controllers
- Update Services

## Implementation Status

- [ ] Run migration script
- [ ] Update Entity models
- [ ] Update DTOs
- [ ] Update Controllers
- [ ] Update Services
- [ ] Update Frontend
- [ ] Test all functionality

## Rollback Plan

If issues occur:
1. Keep old PK columns intact during migration
2. Run both systems in parallel
3. Validate data integrity
4. Only drop old columns after full validation

## Notes

- Primary keys (Id columns) remain for internal database use
- All foreign key relationships now use alternate keys
- Existing data is preserved and migrated
- No data loss occurs during migration
