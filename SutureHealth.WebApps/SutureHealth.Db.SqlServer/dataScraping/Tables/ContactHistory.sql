CREATE TABLE [dataScraping].[ContactHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL,
    [ContactText] NVARCHAR(50) NULL, 
    [ContactType] NVARCHAR(50) NULL, 
    [PreferenceOrder] INT NULL,
    CONSTRAINT [FK_Contact_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 

)
