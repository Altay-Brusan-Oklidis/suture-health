CREATE TABLE [stage].[ZipCode] (
    [ZipCodeId]     BIGINT         IDENTITY (1, 1) NOT NULL,
    [ZIP]           NVARCHAR (5)   NULL,
    [BeginingRoute] NVARCHAR (450) NULL,
    [EndingRoute]   NVARCHAR (450) NULL,
    [Latitude]      DECIMAL (9, 6) NOT NULL,
    [Longitude]     DECIMAL (9, 6) NOT NULL,
    CONSTRAINT [PK_zipcode] PRIMARY KEY CLUSTERED ([ZipCodeId] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_zipcode_ZIP_beg4_end4]
    ON [stage].[ZipCode]([ZIP] ASC, [BeginingRoute] ASC, [EndingRoute] ASC);

