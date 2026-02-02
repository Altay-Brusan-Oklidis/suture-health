CREATE TABLE [dataScraping].[ScrapedPatientHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL, 
    [FirstName] NVARCHAR(50) NULL, 
    [MiddleName] NVARCHAR(50) NULL, 
    [LastName] NVARCHAR(50) NULL, 
    [Phone] NVARCHAR(50) NULL, 
    [SSN] NVARCHAR(50) NULL, 
    [DateOfBirth] DATETIME NULL, 
    [ExternalId] NVARCHAR(50) NULL, 
    [AttendedPhysician] NVARCHAR(50) NULL, 
    [URL] NVARCHAR(250) NULL, 
    [CreatedAt] DATETIME NULL, 
    CONSTRAINT [PK_ScrapedPatientHistory] PRIMARY KEY ([Id]) 
)
