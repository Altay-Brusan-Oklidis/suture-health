CREATE TABLE [dataScraping].[ConditionHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL, 
    [Diagnosis] NVARCHAR(250) NULL, 
    [DiagnosisCode] NVARCHAR(250) NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL,
    CONSTRAINT [FK_Condition_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 

)
