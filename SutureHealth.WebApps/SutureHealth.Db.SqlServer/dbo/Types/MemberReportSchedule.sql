CREATE TYPE [dbo].[MemberReportSchedule] AS TABLE
(
    [ReportTypeId]      SMALLINT,
    [Channel]           TINYINT,
    [Frequency]         TINYINT,
    [Interval]          TINYINT,
    [DaysOfWeek]        TINYINT,
    [DayOfMonth]        TINYINT,
    [MonthsOfYear]      SMALLINT,
    [Time]              TIME(7)
)
