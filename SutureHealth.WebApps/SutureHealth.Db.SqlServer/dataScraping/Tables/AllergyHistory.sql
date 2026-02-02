CREATE TABLE [dataScraping].[AllergyHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NULL,
    [Name] NVARCHAR(250) NULL, 
    [Code] NVARCHAR(250) NULL, 
    [Reaction] NVARCHAR(250) NULL, 
    [Severity] NVARCHAR(250) NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL,
    CONSTRAINT [FK_Allergy_ToScrapedPatientDetailHistory] FOREIGN KEY ([PatientId]) REFERENCES [dataScraping].ScrapedPatientDetailHistory([Id]), 

)
