CREATE FUNCTION [dbo].[CalculateClaimsCountBetweenProviders]
(
	@providerId			bigint,
	@senderFacilityId	int,
	@signerUserId		int
)
RETURNS BIGINT
AS
BEGIN
	DECLARE @totalClaims BIGINT = 0;

	IF @senderFacilityId IS NOT NULL
		BEGIN
			SELECT @totalClaims = SUM(mscpfp.ClaimCount)
			  FROM shapi.MedicareClaimSummaryByProviderAndFileAndPhysician mscpfp
			 INNER JOIN shapi.Provider p ON mscpfp.ProviderOrganizationNPI = p.NPI
			 WHERE mscpfp.ProviderId = @providerId
			   AND p.SutureFacilityId = @senderFacilityId;
		END
	ELSE IF @signerUserId IS NOT NULL
		BEGIN
			SELECT @totalClaims = SUM(mscpfp.ClaimCount)
			  FROM shapi.MedicareClaimSummaryByProviderAndFileAndPhysician mscpfp
			 INNER JOIN GetSigningMembersByUserId(@signerUserId) p ON mscpfp.PhysicianNPI = p.Npi
			 WHERE mscpfp.ProviderId = @providerId;
		END
	ELSE
		BEGIN
			SELECT @totalClaims = SUM(mscp.ClaimCount)
			  FROM shapi.MedicareClaimSummaryByProvider mscp
			 WHERE mscp.ProviderId = @providerId;
		END


	RETURN @totalClaims;
END
GO