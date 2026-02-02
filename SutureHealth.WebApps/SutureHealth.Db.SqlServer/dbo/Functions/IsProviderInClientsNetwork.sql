CREATE FUNCTION [dbo].[IsProviderInClientsNetwork]
(
	@providerId	bigint,
	@sender		int,
	@signer		int
)
RETURNS BIT
AS
BEGIN
	DECLARE @IsInClientNetwork BIT = 0;

	SELECT @IsInClientNetwork =
	   CASE
			WHEN @signer IS NOT NULL AND EXISTS(SELECT TOP 1 1
												  FROM shapi.ProviderSignatureActivitySummary r,
													   dbo.GetSigningMembersByUserId(@signer) s
												 WHERE r.ProviderSenderId = @providerId AND (r.ProviderSignerId = s.ProviderId OR r.SutureSignerId = s.UserId)) THEN 1
			WHEN @sender IS NOT NULL AND EXISTS(SELECT 1
												  FROM shapi.ProviderSignatureActivitySummary r
											     WHERE (r.ProviderSignerId = @providerId AND (r.ProviderSenderId = @sender OR r.SutureSenderId = @sender))) THEN 1
			ELSE 0
		END

	RETURN @IsInClientNetwork;
END
GO