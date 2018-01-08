SELECT 
	--OBJECT_SCHEMA_NAME(i.object_id) AS SchemaName,
	--OBJECT_NAME(i.object_id) AS TableName,
	c.name AS ColumnName
	--ic.key_ordinal AS KeyOrdinal
FROM sys.key_constraints AS kc
JOIN sys.indexes AS i ON i.object_id = kc.parent_object_id AND kc.name = i.name
JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE 
	kc.type_desc = N'PRIMARY_KEY_CONSTRAINT' AND
	OBJECT_SCHEMA_NAME(i.object_id) = @schema AND
	OBJECT_NAME(i.object_id) = @table
ORDER BY
    --SchemaName,
	--TableName,
	--KeyOrdinal;
	ic.key_ordinal;