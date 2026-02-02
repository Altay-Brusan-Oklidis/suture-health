CREATE FUNCTION [dbo].[ParseNumbers]
(
	@Input NVARCHAR(MAX)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	WHILE PATINDEX('%[^0-9]%', @Input) > 0
    BEGIN
        SET @Input = STUFF(@Input, PATINDEX('%[^0-9]%', @Input), 1, '')
    END
    RETURN @Input;
END
