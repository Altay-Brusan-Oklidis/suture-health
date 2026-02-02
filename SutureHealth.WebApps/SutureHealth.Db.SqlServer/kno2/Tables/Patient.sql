CREATE TABLE [kno2].[Patient]
(
    [Id] BIGINT NOT NULL IDENTITY, 
    [ObfuscatedId] CHAR(40) NOT NULL, 
    [ObfuscatedPatientIdRoot] VARCHAR(50) NULL, 
    [PatientIds] VARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data
    [FirstName] NVARCHAR(50) NULL, 
    [MiddleName] NVARCHAR(50) NULL, 
    [LastName] NVARCHAR(50) NULL, 
    [Suffix] NVARCHAR(10) NULL, 
    [Gender] CHAR NULL, 
    [VisitId] NVARCHAR(50) NULL, 
    [Issuer] NVARCHAR(100) NULL, 
    [BirthDate] DATE NULL, 
    [VisitDate] DATETIME2(2) NULL, 
    [FullName] NVARCHAR(160) NULL, 
    [Zip] VARCHAR(10) NULL, -- deprecated
    [StreetAddress1] NVARCHAR(100) NULL, 
    [StreetAddress2] NVARCHAR(50) NULL, 
    [City] NVARCHAR(100) NULL, 
    [State] NVARCHAR(100) NULL, 
    [PostalCode] NVARCHAR(10) NULL, 
    [Country] NVARCHAR(52) NULL, -- TODO: check if codes are used
    [Telephone] VARCHAR(15) NULL,
    [IntegrationMeta] NVARCHAR(MAX) NULL, -- TODO: we hold this as json string - think how to normalize it once we have some data. There is the same column in Attachment and AttachmentMeta - check if they are the same.
    CONSTRAINT [PK_Patient] PRIMARY KEY ([Id]),
    CONSTRAINT [UK_Patient_ObfuscatedId] UNIQUE ([ObfuscatedId]),
)
