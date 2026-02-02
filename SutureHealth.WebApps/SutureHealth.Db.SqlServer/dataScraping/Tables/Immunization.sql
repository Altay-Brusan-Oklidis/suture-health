CREATE TABLE [dataScraping].[Immunization]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL, 
    [Name] NVARCHAR(250) NULL, 
    [Code] NVARCHAR(250) NULL, 
    [AdministrationDate] DATETIME NULL, 
    [ExpirationDate] DATETIME NULL, 
    CONSTRAINT [FK_Immunization_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    CONSTRAINT [PK_Immunization] PRIMARY KEY ([Id])
)
