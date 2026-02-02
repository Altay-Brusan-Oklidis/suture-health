CREATE TABLE [dbo].[MemberHash]
(
	[MemberHashId] INT NOT NULL PRIMARY KEY IDENTITY,
	[MemberId] INT NOT NULL,
	[HashProvider] nvarchar(128) NOT NULL,
	[CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
	[Name] nvarchar(128) NOT NULL,
	[Value] nvarchar(MAX) NOT NULL
)
GO

CREATE UNIQUE INDEX [UX_MemberHash_MemberProviderCreationName] ON [dbo].[MemberHash] 
	([MemberId],[HashProvider],[CreatedAt],[Name])
