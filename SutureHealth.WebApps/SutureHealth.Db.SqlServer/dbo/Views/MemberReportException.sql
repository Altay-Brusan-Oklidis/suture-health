CREATE VIEW MemberReportException
AS
SELECT mi.MemberId,
       rt.ReportTypeId,
       rs.Channel         ReportChannelId,
       CAST(0 AS TINYINT) DaysOfWeek
  FROM dbo.MemberIdentity mi 
 CROSS JOIN dbo.ReportType rt
 INNER JOIN dbo.ReportSchedule rs ON rt.ReportTypeId = rs.ReportTypeId
 WHERE rs.Channel = 2
   AND mi.IsActive = 1        
   AND COALESCE(mi.MobileNumberConfirmed, 0) = 0
        