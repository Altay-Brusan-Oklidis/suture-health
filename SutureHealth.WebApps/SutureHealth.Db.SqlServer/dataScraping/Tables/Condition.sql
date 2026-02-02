CREATE TABLE [dataScraping].[Condition]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [Diagnosis] NVARCHAR(250) NULL, 
    [DiagnosisCode] NVARCHAR(250) NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL,     
    CONSTRAINT [FK_Condition_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    CONSTRAINT [PK_Condition] PRIMARY KEY ([Id])
)
