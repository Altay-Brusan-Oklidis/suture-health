CREATE TABLE [dataScraping].[MedicationHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(250) NULL, 
    [Code] NVARCHAR(250) NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL,
    CONSTRAINT [FK_Medication_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 

)
