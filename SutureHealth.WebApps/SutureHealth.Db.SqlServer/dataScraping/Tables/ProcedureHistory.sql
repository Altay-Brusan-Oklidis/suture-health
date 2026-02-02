CREATE TABLE [dataScraping].[ProcedureHistory]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [PatientId] UNIQUEIDENTIFIER NOT NULL, 
    [Issue] NVARCHAR(250) NULL, 
    [Reason] NVARCHAR(250) NULL, 
    [Provider] NVARCHAR(50) NULL, 
    [Billing] NVARCHAR(250) NULL, 
    [Insurance] NVARCHAR(250) NULL, 
    [Date] DATETIME NULL,
    CONSTRAINT [FK_Procedure_ScrapedPatientDetailHistory] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetailHistory]([Id]), 
)
