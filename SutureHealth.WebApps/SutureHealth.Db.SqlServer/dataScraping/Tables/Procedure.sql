CREATE TABLE [dataScraping].[Procedure]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [Issue] NVARCHAR(250) NULL, 
    [Reason] NVARCHAR(250) NULL, 
    [Provider] NVARCHAR(50) NULL, 
    [Billing] NVARCHAR(250) NULL, 
    [Insurance] NVARCHAR(250) NULL, 
    [Date] DATETIME NULL, 
    CONSTRAINT [FK_Procedure_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    CONSTRAINT [PK_Procedure] PRIMARY KEY ([Id])
)
