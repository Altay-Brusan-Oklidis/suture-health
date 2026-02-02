CREATE VIEW [import].suture_signature_activity_summary
AS
SELECT CAST(uf_signer.UserId as bigint)     as SutureSignerId
    ,  CAST(uf_sender.FacilityId as bigint) as SutureSenderId
    ,  COUNT(1) as count
    ,  CONVERT(date, MAX(r.TimeStamp)) as last_interaction
  FROM [$(SutureSignWeb)].dbo.[Requests] r WITH (NOLOCK)
  JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf_sender WITH (NOLOCK) ON uf_sender.Id = r.Submitter
  JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf_signer WITH (NOLOCK) ON uf_signer.Id = r.Signer
 WHERE r.TimeStamp > DATEADD(month, -12, GETUTCDATE())
 GROUP BY uf_sender.FacilityId, uf_signer.UserId
