SELECT
	[name] as column_name,
	seed_value, 
	increment_value, 
	last_value,
	user_type_id,
	max_length,
	is_computed
FROM     
	SYS.IDENTITY_COLUMNS 
WHERE 
	OBJECT_SCHEMA_NAME(OBJECT_ID) = @schema AND
	OBJECT_NAME(OBJECT_ID) = @table