CREATE VIEW [dbo].[SignerDigestReportData]
AS 
	WITH SignerNotifications(UserId, UserTypeId)
	AS
	(
	SELECT DISTINCT a.UserId, b.UserTypeId
	  FROM [$(SutureSignWeb)].dbo.Tasks a 
	  JOIN [$(SutureSignWeb)].dbo.Users b ON b.UserId = a.UserId AND a.ActionId = 553 and a.formid > 0 and a.Active = 1
	  LEFT JOIN [$(SutureSignWeb)].dbo.Tasks s ON a.FormId = s.FormId AND s.ActionId IN (529, 549, 548, 541, 547, 528, 555, 556,557) AND s.Active = 1 
	  LEFT JOIN [$(SutureSignWeb)].dbo.UserSettings us ON b.UserId = us.UserId and US.Setting = 'EmailTrigger' and US.ItemVarChar = 'ApprovedOnly' and US.Active = 1
	  LEFT JOIN [$(SutureSignWeb)].dbo.Tasks t ON a.FormId = t.FormId AND t.ActionId = 525 AND t.Active = 1
	 WHERE b.Active = 1
	   AND b.CanSign = 1
	   AND s.TaskId IS NULL
	   AND us.UserId IS NULL
	   AND CASE
			  WHEN us.UserId IS NULL OR t.FormId IS NOT NULL THEN 1
			  ELSE 0
		   END = 1
	)
	SELECT sn.UserId									AS MemberId,
		   0											AS TotalRejected,
		   SUM(CASE
                    WHEN US.ItemVarChar IS NULL THEN 1
                    WHEN us.ItemVarChar like 'All|True%' THEN 1
                    WHEN us.ItemVarChar like 'All|False%' THEN 
						CASE
							WHEN sn.UserTypeId = 2000 THEN
								CASE 
									WHEN r.AT IS NOT NULL THEN
										CASE
											WHEN CT IS NULL AND HT IS NULL THEN 1
											WHEN HT IS NULL AND COALESCE(au.IsCollaborator, 0) = 1 THEN 1
											ELSE 0
										END
									WHEN r.AT IS NULL THEN
										CASE
											WHEN CT IS NULL AND HT IS NULL THEN 1
											ELSE 0
										END
								END                
							ELSE
								CASE 
									WHEN r.HT IS NULL AND r.CT IS NOT NULL AND ct.UserId = sn.UserId AND r.AT IS NULL THEN 1
									WHEN r.HT IS NULL AND r.CT IS NOT NULL AND ct.UserId = sn.UserId AND r.AT IS NOT NULL AND COALESCE(au.IsCollaborator, 0) = 0 THEN 1
									WHEN r.HT IS NOT NULL AND ht.UserId = sn.UserId THEN 1
									WHEN r.Signer IS NOT NULL AND sfm.UserId = sn.UserId THEN 1
									ELSE 0
								END                

						END
                    WHEN us.ItemVarChar like 'Approved|True%'  and (R.At is not null) THEN 1
                    WHEN us.ItemVarChar like 'Approved|False%' and (R.At is not null)  THEN 
						CASE
							WHEN sn.UserTypeId = 2000 THEN
								CASE 
									WHEN CT IS NULL AND HT IS NULL THEN 1
									ELSE COALESCE(au.IsCollaborator, 0)
								END                
							ELSE
								CASE 
									WHEN r.HT IS NULL AND r.CT IS NOT NULL AND ct.UserId = sn.UserId AND r.AT IS NULL THEN 1
									WHEN r.HT IS NULL AND r.CT IS NOT NULL AND ct.UserId = sn.UserId AND r.AT IS NOT NULL AND COALESCE(au.IsCollaborator, 0) = 0 THEN 1
									WHEN r.HT IS NOT NULL AND ht.UserId = sn.UserId THEN 1
									WHEN r.Signer IS NOT NULL AND sfm.UserId = sn.UserId THEN 1
									ELSE 0
								END                
						END
                    WHEN us.ItemVarChar like 'Unapproved|True%'  and (R.At is null) THEN 1
                    WHEN us.ItemVarChar like 'Unapproved|False%' and (R.At is null) THEN 
						CASE
							WHEN sn.UserTypeId = 2000 THEN
								CASE 
									WHEN CT IS NULL AND HT IS NULL THEN 1
									ELSE 0
								END
							ELSE
								CASE 
									WHEN r.CT IS NOT NULL AND ct.UserId = sn.UserId AND r.HT IS NULL AND r.AT IS NOT NULL AND COALESCE(au.IsCollaborator, 0) = 0 THEN 1
									WHEN r.CT IS NOT NULL AND ct.UserId = sn.UserId AND r.HT IS NULL AND r.AT IS NULL THEN 1
									WHEN r.HT IS NOT NULL AND ht.UserId = sn.UserId THEN 1
									WHEN r.Signer IS NOT NULL AND sfm.UserId = sn.UserId THEN 1
									ELSE 0
								END                
						END
                    ELSE 0
			   END)										AS TotalSigned,
		   COUNT(*)										AS TotalPending,
		   COALESCE(SUM(Rates.Rate), 0)					AS PotentialAmount,
		   COALESCE(SUM(Rates.RelativeValueUnit), 0)	AS RelativeValueUnit,
		   '[]'											AS RejectedCounts,
		   '[]'											AS SignedCounts,
		   (SELECT *
			  FROM (SELECT  0 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) BETWEEN  0 AND  4 THEN 1 ELSE 0 END) [Count]
					UNION ALL 
					SELECT  5 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) BETWEEN  5 AND  9 THEN 1 ELSE 0 END) [Count]
					UNION ALL 
					SELECT 10 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) BETWEEN 10 AND 14 THEN 1 ELSE 0 END) [Count]
					UNION ALL 
					SELECT 15 Days, SUM(CASE WHEN DATEDIFF(d, r.TimeStamp, GETDATE()) > 14 THEN 1 ELSE 0 END) [Count]) a
			FOR JSON PATH)								AS PendingCounts
	  FROM SignerNotifications sn
	 INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON sn.UserId = uf.UserId AND uf.Active = 1
	 INNER JOIN [$(SutureSignWeb)].dbo.Requests r ON r.Signer = uf.Id AND r.Status IS NULL AND r.Disabled = 0 AND r.Id > 1
   	  LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities sfm ON r.Signer = sfm.Id AND sfm.Active = 1
      LEFT JOIN [$(SutureSignWeb)].dbo.Tasks [at] ON r.[AT] = [at].TaskId
      LEFT Join [$(SutureSignWeb)].dbo.Tasks CT ON R.Ct = CT.TaskId
      LEFT Join [$(SutureSignWeb)].dbo.Tasks HT ON R.Ht = HT.TaskId
	  LEFT JOIN [$(SutureSignWeb)].dbo.Users au ON at.SubmittedBy = au.UserId
	  LEFT JOIN [$(SutureSignWeb)].dbo.Templates T ON T.TemplateId = R.Template
	  LEFT JOIN [$(SutureSignWeb)].dbo.TemplateProperties TP ON TP.TemplatePropertyId = T.TemplatePropertyId
	  LEFT JOIN [$(SutureSignWeb)].dbo.Rates ON Rates.TemplatePropertyId = TP.TemplatePropertyId And Rates.Active = 1
	  LEFT JOIN [$(SutureSignWeb)].dbo.UserSettings us ON us.UserId = sn.UserId AND US.Setting = 'ViewFormsByStatus' and US.Active = 1
	 GROUP BY sn.UserId