-- Collect Schema Data
select 
	schema_id SchemaID,
	name as SchemaName
from sys.schemas

-- Collect Table Data
select 
	object_id TableID, 
	name as TableName 
from sys.tables 
where 
	Name not like 'AspNet%' and 
	Name not like 'sys%' order by Name

-- Collect Column Data
select 
	object_id TableID, 
	cols.name as ColumnName, 
	types.name as TypeName, 
	cols.is_nullable as Nullable,
	cols.max_length as ByteLength,
	cols.precision as Precision,
	cols.scale as Scale
from  
	sys.all_columns cols 
	INNER JOIN sys.types types ON cols.system_type_id = types.user_type_id 
WHERE object_id = <TABLE_ID>