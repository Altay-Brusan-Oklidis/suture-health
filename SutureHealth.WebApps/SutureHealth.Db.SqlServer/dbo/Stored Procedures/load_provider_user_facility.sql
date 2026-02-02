

-- =============================================
-- Author:		  kkilburn
-- Create date: 2020-11-03
-- Description:	loads final data table from raw imports
-- =============================================
CREATE PROCEDURE [dbo].[load_provider_user_facility]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT Statements.
	SET NOCOUNT ON;

	INSERT INTO [stage].[ProviderRelationship]
			   ([ProviderRelationshipId]
			   ,[SutureUserId]
			   ,[SutureFacilityId]
			   ,[Primary]
			   ,[CreatedAt]
			   ,[EffectiveDate]
			   ,[SubmittedById]
			   ,[Admin]
			   ,[EditProfile]
			   ,[ActiveUpdate]
			   ,[CanSign]
			   ,[UserProviderId]
			   ,[FacilityProviderId])
	SELECT suf.SutureUserFacilityId,
		   suf.SutureUserFacilityUserId,
		   suf.SutureUserFacilityFacilityId,
		   suf.[Primary],
		   suf.CreatedAt,
		   suf.EffectiveAt,
		   suf.SubmittedByUserId,
		   suf.[Admin],
		   suf.EditProfile,
		   suf.ActiveUpdate,
		   suf.CanSign,
		   usr.ProviderId,
		   fac.ProviderId
	  FROM [import].suture_user_facility suf
	 INNER JOIN [stage].[Provider] fac ON suf.SutureUserFacilityFacilityId = fac.SutureFacilityId
	 INNER JOIN [stage].[Provider] usr ON suf.SutureUserFacilityUserId = usr.SutureUserId

END
