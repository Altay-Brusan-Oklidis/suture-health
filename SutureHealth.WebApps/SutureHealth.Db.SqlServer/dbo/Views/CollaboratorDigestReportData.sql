CREATE VIEW [dbo].[CollaboratorDigestReportData]
AS 
SELECT u.UserId									    AS MemberId,
       0											AS TotalRejected,
       SUM(
       CASE
            WHEN US.ItemVarChar IS NULL THEN 1
            WHEN US.ItemVarChar like 'All|True%' THEN 1
            WHEN US.ItemVarChar like 'All|False%' THEN 
                CASE 
                    WHEN r.CT IS NOT NULL AND ct.UserId = u.UserId AND r.HT IS NULL AND r.AT IS NOT NULL AND COALESCE(u.IsCollaborator, 0) = 1 THEN 1
                    WHEN r.CT IS NOT NULL AND ct.UserId = u.UserId AND r.HT IS NULL AND r.AT IS NULL THEN 1
                    WHEN r.HT IS NOT NULL AND ht.UserId = u.UserId THEN 1
                    WHEN r.Signer IS NOT NULL AND ufm.UserId = u.userid THEN 1
                    ELSE 0
                END                
            WHEN US.ItemVarChar like 'Approved|True%' AND [AT].TaskId is not null THEN 1
            WHEN US.ItemVarChar like 'Approved|False%' AND [AT].TaskId is not null THEN
                CASE 
                    WHEN r.CT IS NOT NULL AND ct.UserId = u.UserId AND r.HT IS NULL AND r.AT IS NOT NULL AND COALESCE(u.IsCollaborator, 0) = 1 THEN 1
                    WHEN r.CT IS NOT NULL AND ct.UserId = u.UserId AND r.HT IS NULL AND r.AT IS NULL THEN 1
                    WHEN r.HT IS NOT NULL AND ht.UserId = u.UserId THEN 1
                    WHEN r.Signer IS NOT NULL AND ufm.UserId = u.userid THEN 1
                    ELSE 0
                END                
            WHEN US.ItemVarChar like 'Unapproved|True%' AND [AT].TaskId is null THEN 1
            WHEN US.ItemVarChar like 'Unapproved|False%' AND [AT].TaskId is null THEN
                CASE 
                    WHEN r.CT IS NOT NULL AND ct.UserId = u.UserId AND r.HT IS NULL AND r.AT IS NOT NULL AND COALESCE(u.IsCollaborator, 0) = 1 THEN 1
                    WHEN r.CT IS NOT NULL AND ct.UserId = u.UserId AND r.HT IS NULL AND r.AT IS NULL THEN 1
                    WHEN r.HT IS NOT NULL AND ht.UserId = u.UserId THEN 1
                    WHEN r.Signer IS NOT NULL AND ufm.UserId = u.userid THEN 1
                    ELSE 0
                END                
            ELSE 0
       END)										    AS TotalSigned,
       COUNT(*)										AS TotalPending,
       COALESCE(SUM(Rates.Rate), 0)					AS PotentialAmount,
       COALESCE(SUM(Rates.RelativeValueUnit), 0)	AS RelativeValueUnit,
       '[]'											AS RejectedCounts,
       '[]'											AS SignedCounts,
       (SELECT *
            FROM (SELECT  0 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) BETWEEN  0 AND  4 AND ufa.FacilityId = ufm.FacilityId THEN 1 ELSE 0 END)  [Count]
                   UNION ALL 
                  SELECT  5 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) BETWEEN  5 AND  9 AND ufa.FacilityId = ufm.FacilityId THEN 1 ELSE 0 END)  [Count]
                   UNION ALL 
                  SELECT 10 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) BETWEEN 10 AND 14 AND ufa.FacilityId = ufm.FacilityId THEN 1 ELSE 0 END) [Count]
                   UNION ALL 
                  SELECT 15 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) > 14 AND ufa.FacilityId = ufm.FacilityId THEN 1 ELSE 0 END) [Count]) a
        FOR JSON PATH)								AS PendingCounts
   FROM [$(SutureSignWeb)].dbo.Users u 
  INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities ufa ON ufa.UserId = u.UserId and ufa.Active = 1
  INNER JOIN [$(SutureSignWeb)].dbo.Users_Managers um ON u.UserId = um.UserId and UM.Active = 1
  INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities ufm ON um.ManagerId = ufm.UserId AND ufa.FacilityId = ufm.FacilityId AND ufm.Active = 1
   LEFT JOIN [$(SutureSignWeb)].dbo.UserSettings us ON us.UserId = um.UserId AND US.Setting = 'ViewFormsByStatus' and US.Active = 1
  INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON ufm.Id = r.Signer AND r.status IS NULL AND r.Disabled = 0 AND r.Id > 1
   LEFT Join [$(SutureSignWeb)].dbo.Tasks [AT] ON R.[At] = [AT].TaskId
   LEFT Join [$(SutureSignWeb)].dbo.Tasks CT ON R.Ct = CT.TaskId
   LEFT Join [$(SutureSignWeb)].dbo.Tasks HT ON R.Ht = HT.TaskId
   LEFT JOIN [$(SutureSignWeb)].dbo.Templates T ON T.TemplateId = R.Template
   LEFT JOIN [$(SutureSignWeb)].dbo.TemplateProperties TP ON TP.TemplatePropertyId = T.TemplatePropertyId
   LEFT JOIN [$(SutureSignWeb)].dbo.Rates ON Rates.TemplatePropertyId = TP.TemplatePropertyId And Rates.Active = 1
  WHERE u.IsCollaborator = 1
  GROUP BY u.UserId
 