


-- =============================================
-- Author:		kkilburn
-- Create date: 2020-11-03
-- Description:	loads final data table from raw imports
-- =============================================
CREATE PROCEDURE [dbo].[load_provider_signature_activity_summary]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT Statements.
	SET NOCOUNT ON;

	insert into [stage].ProviderSignatureActivitySummary
	select sender.ProviderId,
		   signer.ProviderId,
		   ssas.SutureSenderId,
		   ssas.SutureSignerId,
		   ssas.count,
		   ssas.last_interaction
	  from [import].suture_signature_activity_summary ssas
	 inner join [stage].provider sender ON ssas.SutureSenderId = sender.SutureFacilityId
	 inner join [stage].provider signer ON ssas.SutureSignerId = signer.SutureUserId;

END
