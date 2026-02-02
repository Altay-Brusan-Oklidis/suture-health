CREATE TABLE [dataScraping].[PrescriptionHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL, 
    [DrugName] NVARCHAR(250) NULL, 
    [Details] NVARCHAR(250) NULL, 
    [Quantity] NVARCHAR(50) NULL, 
    [Refills] NVARCHAR(250) NULL, 
    [FillDate] DATE NULL
    CONSTRAINT [FK_Prescription_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 
)
