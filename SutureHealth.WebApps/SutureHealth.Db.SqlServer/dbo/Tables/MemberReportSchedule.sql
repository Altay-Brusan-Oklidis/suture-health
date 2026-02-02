CREATE TABLE [dbo].[MemberReportSchedule]
(
	[MemberReportScheduleId]	INT			NOT NULL IDENTITY(1, 1) CONSTRAINT PK_MemberReportSchedule PRIMARY KEY CLUSTERED,
	[ReportTypeId]				SMALLINT	NOT NULL CONSTRAINT FK_MemberReportSchedule_ReportTypeId FOREIGN KEY REFERENCES dbo.ReportType([ReportTypeId]),
	[Channel]					TINYINT		NOT NULL,
	[MemberId]					INT			NULL,
	[Frequency]					TINYINT		NULL, 
	[Interval]					TINYINT		NULL, 
	[DaysOfWeek]				TINYINT		NULL, 
	[DayOfMonth]				TINYINT		NULL, 
	[MonthsOfYear]				SMALLINT	NULL,
	[Time]						TIME		NULL,
)
