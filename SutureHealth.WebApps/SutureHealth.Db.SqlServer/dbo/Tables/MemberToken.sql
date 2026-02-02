CREATE TABLE [dbo].[MemberToken]
(
	[MemberTokenId] INT NOT NULL PRIMARY KEY IDENTITY,
	[MemberId] INT NOT NULL,
	[TokenProvider] nvarchar(128) NOT NULL,
	[Name] nvarchar(128) NOT NULL,
	[Value] nvarchar(MAX) NOT NULL
)

GO

CREATE UNIQUE INDEX [UX_MemberToken_MemberProviderName] ON [dbo].[MemberToken] 
	([MemberId],[TokenProvider],[Name])
