CREATE VIEW [dbo].[SenderDigestReportData]
AS 
SELECT F.FacilityId   AS OrganizationId,
       COUNT(CASE 
                WHEN R.[Status] = 1 THEN 1 
                ELSE NULL 
             END)               AS TotalSigned,
       COUNT(CASE 
                WHEN R.[Status] = 2 THEN 1 
                ELSE NULL 
             END)               AS TotalRejected,
       COUNT(CASE 
                WHEN R.Id IS NOT NULL AND R.St IS NULL THEN 1 
                ELSE NULL 
             END)	            AS TotalPending,
       '[]'                     AS RejectedCounts,
	   '[]'                     AS SignedCounts,
      (SELECT 20 [Days], 
              COUNT(CASE 
                        WHEN R.St IS NULL AND DATEDIFF(DAY, ISNULL(R.EffDate, R.StartOfCare), GETDATE()) > 20 THEN 1 
                        ELSE NULL 
                    END) [Count]
         FROM [$(SutureSignWeb)].dbo.Tasks T2
        INNER JOIN [$(SutureSignWeb)].dbo.Requests R ON R.Id = T2.FormId 
                                                    AND R.SubmArchive IS NULL 
                                                    AND R.[Disabled] = 0
    	WHERE T2.ActionId = 551 
    	  AND T2.SubmittedByFacility = F.FacilityId
		  FOR JSON PATH)        AS PendingCounts

  FROM [$(SutureSignWeb)].dbo.Facilities F
  LEFT JOIN [$(SutureSignWeb)].dbo.Tasks T1 ON F.FacilityId = T1.SubmittedByFacility AND T1.ActionId = 551
  LEFT JOIN [$(SutureSignWeb)].dbo.Requests R ON R.Id = T1.FormId 
                                             AND R.SubmArchive IS NULL 
                                             AND R.[Disabled] = 0
 WHERE F.Active = 1
 GROUP BY F.FacilityId