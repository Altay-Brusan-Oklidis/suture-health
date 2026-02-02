CREATE TABLE [dataScraping].[Observation]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [Labs] NVARCHAR(250) NULL, 
    [Vitals] NVARCHAR(250) NULL,     
    CONSTRAINT [FK_Observation_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    CONSTRAINT [PK_Observation] PRIMARY KEY ([Id])
)
