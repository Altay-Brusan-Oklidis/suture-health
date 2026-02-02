CREATE TABLE [dataScraping].[Prescription]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [DrugName] NVARCHAR(250) NULL, 
    [Details] NVARCHAR(250) NULL, 
    [Quantity] NVARCHAR(50) NULL, 
    [Refills] NVARCHAR(250) NULL, 
    [FillDate] DATE NULL, 
    CONSTRAINT [FK_Prescription_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    CONSTRAINT [PK_Prescription] PRIMARY KEY ([Id])
)
