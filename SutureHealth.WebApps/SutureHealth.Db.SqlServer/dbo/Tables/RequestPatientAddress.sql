CREATE TABLE [dbo].[RequestPatientAddress]
(
    [RequestPatientAddressId] BIGINT IDENTITY NOT NULL PRIMARY KEY,
    [RequestPatientId] BIGINT NOT NULL,
    [Line1] nvarchar(100) NULL,
    [Line2] nvarchar(100) NULL,
    [City] nvarchar(60) NULL,
    [County] nvarchar(60) NULL,
    [StateOrProvince] nvarchar(2) NULL,
    [CountryOrRegion] nvarchar(2) NULL,
    [PostalCode] nvarchar(16) NULL
);
GO

CREATE INDEX [IX_RequestPatientAddress_RequestPatientId] ON [dbo].[RequestPatientAddress] ([RequestPatientId]);
GO