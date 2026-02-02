CREATE TABLE [dbo].[MemberPermissionStatement]
(
    [MemberPermissionStatementId] INT NOT NULL PRIMARY KEY,
    [MemberId] INT NOT NULL,
    [ActionIdentifierId] INT NOT NULL,
    [Effect] TINYINT NOT NULL,
    [ResourceIdentifierKey] VARCHAR(1024) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] INT NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [UpdatedBy] INT NOT NULL,
    [DeletedAt] datetime2 NOT NULL,
    [DeletedBy] INT NOT NULL
)
