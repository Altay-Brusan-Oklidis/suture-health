CREATE PROCEDURE [dbo].[execute_provider_data_load]
	@include_zips int = 0
AS
BEGIN

  EXECUTE dbo.remove_foreign_key_contraints @schema = 'stage';

  -- THIS SHOULD ALWAYS BE CLEAN 
  TRUNCATE TABLE stage.[MedicareClaimSummaryByProvider] 
  TRUNCATE TABLE stage.[MedicareClaimSummaryByProviderAndFile]	
  TRUNCATE TABLE stage.[MedicareClaimSummaryByProviderAndFileAndPhysician]
  TRUNCATE TABLE stage.[ProviderService]
  TRUNCATE TABLE stage.[ProviderSignatureActivitySummary]
  TRUNCATE TABLE stage.[ProviderRelationship]
  TRUNCATE TABLE stage.[Provider] 

  IF @include_zips = 1
  BEGIN
    TRUNCATE TABLE stage.zipcode
  END

  -- REMOVE INDEXES
  EXEC dbo.toggle_indexes @schema = 'stage', @enable = 0

  IF @include_zips = 1
  BEGIN
    EXECUTE [dbo].[load_zipcode];
  END

  EXECUTE [dbo].[load_provider]
  EXECUTE [dbo].[load_provider_service]
  EXECUTE [dbo].[load_provider_signature_activity_summary]
  EXECUTE [dbo].[load_provider_user_facility]
  EXECUTE [dbo].[load_provider_medicare_claims]

  -- BUILDS INDEXES
  EXEC dbo.clone_foreign_key_constraints @sourceSchema = 'shapi',
                                         @targetSchema = 'stage';
  EXEC dbo.toggle_indexes @schema = 'stage',
                          @enable = 1

END