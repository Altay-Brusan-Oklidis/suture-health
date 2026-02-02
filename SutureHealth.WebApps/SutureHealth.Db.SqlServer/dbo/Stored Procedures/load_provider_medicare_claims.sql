

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[load_provider_medicare_claims]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT Statements.
	SET NOCOUNT ON;

	INSERT INTO [stage].[MedicareClaimSummaryByProvider]
	(
		 [ProviderId]
		,[ProviderOrganizationNPI]
		,[ClaimCount]
		,[LatestClaimDate]
	)
	SELECT p.ProviderId
		 , [provider_organization_npi]
		 , [claim_count]
		 , [max_claim_date]
	  FROM [import].[medicare_claims_by_provider] imp
	 INNER JOIN [stage].[Provider] p on imp.[provider_organization_npi] = p.npi

	INSERT INTO [stage].[MedicareClaimSummaryByProviderAndFile]
	(
		 [ProviderId]
		,[ProviderOrganizationNPI]
		,[File]
		,[ClaimCount]
		,[LatestClaimDate]
	)
	SELECT p.ProviderId
		 , [provider_organization_npi]
		 , [file]
		 , [claim_count]
		 , [max_claim_date]
	  FROM [import].[medicare_claims_by_provider_and_file] imp
	 INNER JOIN [stage].[Provider] p on imp.[provider_organization_npi] = p.npi

	INSERT INTO [stage].[MedicareClaimSummaryByProviderAndFileAndPhysician]
	(
		 [ProviderId]
		,[ProviderOrganizationNPI]
		,[File]
		,[PhysicianNPI]
		,[ClaimCount]
		,[LatestClaimDate]
	)
	SELECT p.ProviderId
		 , [provider_organization_npi]
		 , [file]
		 , [physician_npi]
		 , [claim_count]
		 , [max_claim_date]
	  FROM [import].[medicare_claims_by_provider_and_file_and_physician] imp
	 INNER JOIN [stage].[Provider] p on imp.[provider_organization_npi] = p.npi

END
