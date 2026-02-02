CREATE PROCEDURE [dbo].[AddOrUpdateMemberReportSchedule]
    @MemberId           INT,
    @Schedule           MemberReportSchedule READONLY
AS
BEGIN

    MERGE dbo.MemberReportSchedule AS t  
    USING @Schedule AS s ON (t.MemberId = @MemberId AND t.ReportTypeId = s.ReportTypeId AND t.Channel = s.Channel)
    WHEN MATCHED
        THEN UPDATE SET [Frequency] = s.[Frequency],
                        [Interval] = s.[Interval],
                        [DaysOfWeek] = s.[DaysOfWeek],
                        [DayOfMonth] = s.[DayOfMonth],
                        [MonthsOfYear] = s.[MonthsOfYear],
                        [Time] = s.[Time]
    WHEN NOT MATCHED BY TARGET
        THEN INSERT([MemberId], [ReportTypeId], [Channel], [Frequency], [Interval], [DaysOfWeek], [DayOfMonth], [MonthsOfYear], [Time])
             VALUES(@MemberId, s.[ReportTypeId], s.[Channel], s.[Frequency], s.[Interval], s.[DaysOfWeek], s.[DayOfMonth], s.[MonthsOfYear], s.[Time]);

END