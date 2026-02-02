CREATE PROCEDURE [dbo].[remove_foreign_key_contraints]
	@schema VARCHAR(16)
AS
BEGIN
  SET NOCOUNT ON;

	DECLARE @TableName VARCHAR(256);
	DECLARE @ConstraintName VARCHAR(256);
	DECLARE @TSQLStatement VARCHAR(MAX);

	DECLARE CursorConstraints CURSOR FAST_FORWARD FOR
   SELECT TABLE_NAME,
          CONSTRAINT_NAME          
     FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
    WHERE TABLE_SCHEMA = @schema
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'

	OPEN CursorConstraints;
	FETCH NEXT FROM CursorConstraints INTO @TableName, @ConstraintName;

	WHILE @@fetch_status = 0
	BEGIN
	  SET @TSQLStatement = 'ALTER TABLE ' + @schema + '.[' + @TableName + '] DROP CONSTRAINT ' + @ConstraintName ;
    EXEC(@TSQLStatement);
	  FETCH NEXT FROM CursorConstraints INTO @TableName, @ConstraintName;
	END

	CLOSE CursorConstraints
	DEALLOCATE CursorConstraints
END