CREATE VIEW [dbo].[ReportMemberSchedule]
AS 
SELECT DISTINCT
       m.MemberId,
       mrs.ReportTypeId,
       COALESCE(mrso.Channel,       mrs.Channel)                    Channel,
       COALESCE(mrso.Frequency,     mrs.Frequency)                  Frequency,  
       COALESCE(mrso.Interval,      mrs.Interval)                   Interval,   
       COALESCE(CAST(mrex.DaysOfWeek AS TINYINT), mrso.DaysOfWeek, mrs.DaysOfWeek)   DaysOfWeek,  
       COALESCE(mrso.[DayOfMonth],  mrs.[DayOfMonth])               [DayOfMonth], 
       COALESCE(mrso.MonthsOfYear,  mrs.MonthsOfYear)               MonthsOfYear,
       mrs.[Time]
  FROM dbo.MemberReportSchedule mrs
 INNER JOIN dbo.ReportEligibility re ON mrs.ReportTypeId = re.ReportTypeId
 INNER JOIN dbo.Member m ON COALESCE(re.MemberTypeId, m.MemberTypeID) = m.MemberTypeId
                              AND COALESCE(re.MemberId, m.MemberId) = m.MemberId
                              AND COALESCE(re.CanSign, m.CanSign) = m.CanSign
                              AND COALESCE(re.IsCollaborator, m.IsCollaborator) = m.IsCollaborator
 INNER JOIN dbo.OrganizationMember om ON m.MemberId = om.MemberId AND om.IsActive = 1 
 INNER JOIN dbo.Organization o ON om.OrganizationId = o.OrganizationId 
                              AND o.OrganizationTypeId = COALESCE(re.OrganizationTypeId, o.OrganizationTypeId)
                              AND o.OrganizationId = COALESCE(re.OrganizationId, o.OrganizationId)
                              AND o.CompanyId = COALESCE(re.CompanyId, o.CompanyId)
  LEFT JOIN dbo.MemberReportSchedule mrso ON mrs.ReportTypeId = mrso.ReportTypeId AND mrso.MemberId IS NOT NULL AND m.MemberId = mrso.MemberId
  LEFT JOIN dbo.MemberReportException mrex ON mrs.MemberId = mrex.MemberId AND mrs.ReportTypeId = mrex.ReportTypeId AND mrs.Channel = ReportChannelId
 WHERE mrs.MemberId IS NULL