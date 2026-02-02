CREATE PROCEDURE dbo.toggle_indexes
	@schema VARCHAR(16),
	@enable BIT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TableName VARCHAR(256);
	DECLARE @IndexName VARCHAR(256);
	DECLARE @TSQLStatement VARCHAR(MAX);

  IF @enable IS NULL
	BEGIN
		RAISERROR('@enable must be 0 or 1', 16, 1);
		RETURN;
	END

	DECLARE CursorIndexes CURSOR FAST_FORWARD FOR
	SELECT t.name,  i.name 
	FROM sys.indexes i
	INNER JOIN sys.tables t ON t.object_id = i.object_id
	WHERE i.type > 1 and t.is_ms_shipped = 0 and t.name <> 'sysdiagrams'
	AND (is_primary_key = 0 and is_unique_constraint = 0)
	AND schema_name(t.schema_id) = @schema;

	OPEN CursorIndexes;
	FETCH NEXT FROM CursorIndexes INTO @TableName, @IndexName;

	WHILE @@fetch_status = 0
	BEGIN
	 SET @TSQLStatement = 'ALTER INDEX ' + @IndexName + ' ON ' + @schema + '.' + @TableName + CASE @enable WHEN 0 THEN ' DISABLE;' WHEN 1 THEN ' REBUILD;' END;
   --PRINT @TSQLStatement
   EXEC(@TSQLStatement);
	 FETCH NEXT FROM CursorIndexes INTO @TableName, @IndexName
	END

	CLOSE CursorIndexes
	DEALLOCATE CursorIndexes
END