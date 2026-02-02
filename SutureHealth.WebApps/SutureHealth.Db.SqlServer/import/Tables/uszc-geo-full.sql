CREATE TABLE [import].[uszc-geo-full] (
    [ZIPCode]    INT            NOT NULL,
    [State]      NVARCHAR (50)  NOT NULL,
    [City]       NVARCHAR (50)  NOT NULL,
    [County]     NVARCHAR (50)  NULL,
    [Latitude]   DECIMAL (9, 6) NOT NULL,
    [Longitude]  DECIMAL (9, 6) NOT NULL,
    [AreaCode]   INT            NOT NULL,
    [StateFIPS]  INT            NOT NULL,
    [CountyFIPS] NVARCHAR (50)  NOT NULL,
    [PlaceFIPS]  INT            NOT NULL,
    [MSACode]    NVARCHAR (50)  NOT NULL,
    [TimeZone]   INT            NOT NULL,
    [DST]        NVARCHAR (50)  NOT NULL,
    [ZIPType]    NVARCHAR (50)  NULL
);

