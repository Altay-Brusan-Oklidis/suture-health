CREATE TABLE [dataScraping].[ObservationHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL,
    [Labs] NVARCHAR(250) NULL, 
    [Vitals] NVARCHAR(250) NULL, 
    CONSTRAINT [FK_Observation_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 
)
