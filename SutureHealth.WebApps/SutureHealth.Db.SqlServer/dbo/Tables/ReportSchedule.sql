CREATE TABLE [dbo].[ReportSchedule]
(
	[ReportScheduleId]		INT			NOT NULL IDENTITY(1, 1) CONSTRAINT PK_ReportSchedule PRIMARY KEY CLUSTERED,
	[ReportTypeId]			SMALLINT	NOT NULL CONSTRAINT FK_ReportSchedule_ReportTypeId FOREIGN KEY REFERENCES dbo.ReportType([ReportTypeId]),
	[Channel]				TINYINT		NOT NULL,
	[Frequency]				TINYINT		NULL, 
	[Interval]				TINYINT		NULL, 
	[DaysOfWeek]			TINYINT		NULL, 
	[DayOfMonth]			TINYINT		NULL, 
	[MonthsOfYear]			SMALLINT	NULL, 
	[Time]					TIME(0)		NULL, 
    [Active] BIT NOT NULL DEFAULT 1
)
