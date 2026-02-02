CREATE TABLE [dataScraping].[Medication]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [Name] NVARCHAR(250) NULL, 
    [Code] NVARCHAR(250) NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    CONSTRAINT [FK_Medication_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    PRIMARY KEY ([Id])
)
