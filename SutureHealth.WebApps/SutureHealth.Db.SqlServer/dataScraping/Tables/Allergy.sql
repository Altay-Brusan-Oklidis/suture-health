CREATE TABLE [dataScraping].[Allergy]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [Name] NVARCHAR(250) NULL, 
    [Code] NVARCHAR(250) NULL, 
    [Reaction] NVARCHAR(250) NULL, 
    [Severity] NVARCHAR(250) NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    CONSTRAINT [FK_Allergy_ToScrapedPatientDetail] FOREIGN KEY ([PatientId]) REFERENCES [dataScraping].ScrapedPatientDetail([Id]), 
    CONSTRAINT [PK_Allergy] PRIMARY KEY ([Id]) 
)
