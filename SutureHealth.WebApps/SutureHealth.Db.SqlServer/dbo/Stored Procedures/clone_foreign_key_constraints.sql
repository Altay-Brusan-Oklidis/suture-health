CREATE PROCEDURE [dbo].[clone_foreign_key_constraints]
    @sourceSchema VARCHAR(16),
    @targetSchema VARCHAR(16)
AS
BEGIN
   SET NOCOUNT ON;

	DECLARE @ConstraintName VARCHAR(256);
	DECLARE @ConstraintTableName VARCHAR(256);
	DECLARE @ConstraintColumns VARCHAR(MAX);
	DECLARE @ReferenceTableName VARCHAR(256);
	DECLARE @ReferenceColumns VARCHAR(MAX);

	DECLARE @TSQLStatement VARCHAR(MAX);

	DECLARE CursorConstraints CURSOR FAST_FORWARD FOR
   SELECT fk.name ConstraintName
        , ct.name ConstraintTableName
        , STUFF((SELECT ',' + quotename(c.name) -- get all the columns in the constraint table
                   FROM sys.columns AS c
                  INNER JOIN sys.foreign_key_columns AS fkc ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.[object_id]
                  WHERE fkc.constraint_object_id = fk.[object_id]
                    FOR XML PATH(''), TYPE).value('.[1]', 'nvarchar(max)')
               , 1, 1, '')
               AS ConstraintColumns
        , rt.name ReferenceTableName
        , STUFF((SELECT ',' + quotename(c.name) -- get all the referenced columns
                   FROM sys.columns AS c
                  INNER JOIN sys.foreign_key_columns AS fkc ON fkc.referenced_column_id = c.column_id AND fkc.referenced_object_id = c.[object_id]
                  WHERE fkc.constraint_object_id = fk.[object_id]
                    FOR XML PATH(''), TYPE).value('.[1]', N'nvarchar(max)')
			   , 1, 1, '')
               AS ReferenceColumns
     FROM sys.foreign_keys AS fk
    INNER JOIN sys.tables  AS rt ON fk.referenced_object_id = rt.[object_id]
    INNER JOIN sys.tables  AS ct ON fk.parent_object_id = ct.[object_id]
    WHERE SCHEMA_NAME(fk.schema_id) = @sourceSchema
      AND rt.is_ms_shipped = 0 
      AND ct.is_ms_shipped = 0

	OPEN CursorConstraints;
	FETCH NEXT FROM CursorConstraints INTO @ConstraintName, @ConstraintTableName, @ConstraintColumns,
                                                          @ReferenceTableName,  @ReferenceColumns;

	WHILE @@fetch_status = 0
	BEGIN

    IF (EXISTS(SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @targetSchema AND TABLE_NAME = @ConstraintTableName) AND
        EXISTS(SELECT TOP 1 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @targetSchema AND TABLE_NAME = @ReferenceTableName))
    BEGIN
	    SET @TSQLStatement = 'ALTER TABLE ' + @targetSchema + '.' + quotename(@ConstraintTableName) 
                         +   ' ADD CONSTRAINT ' + @ConstraintName + ' FOREIGN KEY(' + @ConstraintColumns + ')'
                                                                  + ' REFERENCES ' + @targetSchema + '.' + quotename(@ReferenceTableName)
                                                                  + '(' + @ReferenceColumns + ');';
	    --PRINT @TSQLStatement
      EXEC(@TSqlStatement);
    END

	  FETCH NEXT FROM CursorConstraints INTO @ConstraintName, @ConstraintTableName, @ConstraintColumns,
                                                            @ReferenceTableName,  @ReferenceColumns;
	END

	CLOSE CursorConstraints
	DEALLOCATE CursorConstraints


END