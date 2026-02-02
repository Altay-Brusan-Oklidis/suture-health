CREATE TABLE [dbo].[TimeZoneValue]
(
	[TimeZoneValueId] INT NOT NULL PRIMARY KEY, 
    [Name] NVARCHAR(50) NULL, 
    [TimeOffset] NVARCHAR(10) NULL
)
