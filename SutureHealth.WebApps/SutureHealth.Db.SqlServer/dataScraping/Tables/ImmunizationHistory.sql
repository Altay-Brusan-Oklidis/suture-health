CREATE TABLE [dataScraping].[ImmunizationHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(250) NULL, 
    [Code] NVARCHAR(250) NULL, 
    [AdministrationDate] DATETIME NULL, 
    [ExpirationDate] DATETIME NULL
    CONSTRAINT [FK_Immunization_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 

)
