-- This function is used in check constraints in tables like MemberMemberGroup and MemberRole.
-- These constraints should be foreign keys to the Users table, but it is in a different database.
-- Until we refactor the tables to be in one database, we will use these checks to provide some data integrity.
CREATE FUNCTION [dbo].[MemberIdExists]
(
    @memberId int
)
RETURNS BIT
AS
BEGIN
    DECLARE @retval BIT
    IF EXISTS (SELECT 1 FROM [$(SutureSignWeb)].dbo.Users u WHERE u.UserId = @MemberId)
        SET @retval = 1
    ELSE
        SET @retval = 0
    RETURN @retval
END
