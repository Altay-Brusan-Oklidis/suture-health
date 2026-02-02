CREATE FUNCTION [dbo].[GetDefaultResourceForPermissionStatement]
(
    @actionIdentifierDomain VARCHAR(128),
    @actionIdentifierKey    VARCHAR(128),
    @domainObjectId         INT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
    DECLARE @resource       VARCHAR(MAX)
    DECLARE @domainObject   VARCHAR(128)

    SET @resource = CASE 
                        WHEN @actionIdentifierDomain = 'member' AND @domainObjectId IS NOT NULL THEN CONCAT('srn:member:*:', @domainObjectId)
                        WHEN @actionIdentifierDomain = 'member' AND @domainObjectId IS NOT NULL THEN CONCAT('srn:member:*:', @domainObjectId)
                        ELSE NULL
                    END

    RETURN @resource
END