-- Check which foreign keys reference primary keys vs alternate keys
SELECT 
    OBJECT_NAME(fk.parent_object_id) AS ForeignKeyTable,
    fk.name AS ForeignKeyName,
    COL_NAME(fk.parent_object_id, fkc.parent_column_id) AS ForeignKeyColumn,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fk.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn,
    CASE 
        WHEN COL_NAME(fk.referenced_object_id, fkc.referenced_column_id) = 'Id' THEN 'PRIMARY KEY (BAD)'
        ELSE 'ALTERNATE KEY (GOOD)'
    END AS ReferenceType
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
ORDER BY 
    CASE WHEN COL_NAME(fk.referenced_object_id, fkc.referenced_column_id) = 'Id' THEN 0 ELSE 1 END,
    OBJECT_NAME(fk.parent_object_id),
    fk.name;
