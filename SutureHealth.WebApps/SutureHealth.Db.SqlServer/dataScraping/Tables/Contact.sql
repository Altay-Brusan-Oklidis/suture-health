CREATE TABLE [dataScraping].[Contact]
(
    [Id] UNIQUEIDENTIFIER NOT NULL, 
	[PatientId] UNIQUEIDENTIFIER NOT NULL , 
    [ContactText] NVARCHAR(50) NULL, 
    [ContactType] INT NULL, 
    [PreferenceOrder] INT NULL, 
    CONSTRAINT [FK_Contact_ScrapedPatientDetail] FOREIGN KEY (PatientId) REFERENCES [dataScraping].[ScrapedPatientDetail]([Id]), 
    CONSTRAINT [PK_Contact] PRIMARY KEY ([Id])

)
