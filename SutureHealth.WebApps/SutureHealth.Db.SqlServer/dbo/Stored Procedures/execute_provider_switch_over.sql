CREATE PROCEDURE [dbo].[execute_provider_switch_over]
	@include_zips int = 0
AS

EXECUTE dbo.remove_foreign_key_contraints @schema = 'archive';

TRUNCATE TABLE archive.[MedicareClaimSummaryByProvider] 
TRUNCATE TABLE archive.[MedicareClaimSummaryByProviderAndFile]	
TRUNCATE TABLE archive.[MedicareClaimSummaryByProviderAndFileAndPhysician] 
TRUNCATE TABLE archive.[ProviderService]
TRUNCATE TABLE archive.[ProviderSignatureActivitySummary]
TRUNCATE TABLE archive.[ProviderRelationship] 
TRUNCATE TABLE archive.[Provider] 
IF @include_zips = 1
BEGIN
  TRUNCATE TABLE archive.zipcode 
END

EXEC dbo.clone_foreign_key_constraints @sourceSchema = 'shapi',
                                       @targetSchema = 'archive';

BEGIN TRANSACTION

EXEC sp_msforeachtable @command1 = 'ALTER TABLE ? NOCHECK CONSTRAINT ALL',
                       @whereand = ' AND schema_name(syso.schema_id) = ''archive''';

EXEC sp_msforeachtable @command1 = 'ALTER TABLE ? NOCHECK CONSTRAINT ALL',
                       @whereand = ' AND schema_name(syso.schema_id) = ''shapi''';

EXEC sp_msforeachtable @command1 = 'ALTER TABLE ? NOCHECK CONSTRAINT ALL',
                       @whereand = ' AND schema_name(syso.schema_id) = ''stage''';


-- ARCHIVE CURRENT DATA
ALTER TABLE shapi.[Provider]									SWITCH TO archive.[Provider]										WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
ALTER TABLE shapi.[MedicareClaimSummaryByProvider]					SWITCH TO archive.[MedicareClaimSummaryByProvider]						WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
ALTER TABLE shapi.[MedicareClaimSummaryByProviderAndFile]				SWITCH TO archive.[MedicareClaimSummaryByProviderAndFile]					WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
ALTER TABLE shapi.[MedicareClaimSummaryByProviderAndFileAndPhysician]	SWITCH TO archive.[MedicareClaimSummaryByProviderAndFileAndPhysician]	    WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
ALTER TABLE shapi.providerservice								SWITCH TO archive.providerservice									WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
ALTER TABLE shapi.providersignatureactivitysummary				SWITCH TO archive.providersignatureactivitysummary					WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
ALTER TABLE shapi.[ProviderRelationship]							SWITCH TO archive.[ProviderRelationship]								WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
IF @include_zips = 1
BEGIN
  ALTER TABLE shapi.zipcode	SWITCH TO archive.zipcode														                        WITH ( WAIT_AT_LOW_PRIORITY ( MAX_DURATION = 1 MINUTES, ABORT_AFTER_WAIT = BLOCKERS ));
END

-- MOVE IN NEW DATA
ALTER TABLE stage.[Provider]										SWITCH TO shapi.[Provider]
ALTER TABLE stage.[MedicareClaimSummaryByProvider]						SWITCH TO shapi.[MedicareClaimSummaryByProvider]
ALTER TABLE stage.[MedicareClaimSummaryByProviderAndFile]					SWITCH TO shapi.[MedicareClaimSummaryByProviderAndFile]
ALTER TABLE stage.[MedicareClaimSummaryByProviderAndFileAndPhysician]	    SWITCH TO shapi.[MedicareClaimSummaryByProviderAndFileAndPhysician]
ALTER TABLE stage.[ProviderService]									SWITCH TO shapi.providerservice
ALTER TABLE stage.[ProviderSignatureActivitySummary]				SWITCH TO shapi.providersignatureactivitysummary
ALTER TABLE stage.[ProviderRelationship]							SWITCH TO shapi.[ProviderRelationship]
IF @include_zips = 1
BEGIN
  ALTER TABLE stage.zipcode	SWITCH TO shapi.zipcode
END

EXEC sp_msforeachtable @command1 = "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL",
                       @whereand = ' AND schema_name(syso.schema_id) = ''stage''';

EXEC sp_msforeachtable @command1 = "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL",
                       @whereand = ' AND schema_name(syso.schema_id) = ''shapi''';

EXEC sp_msforeachtable @command1 = "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL",
                       @whereand = ' AND schema_name(syso.schema_id) = ''archive''';

COMMIT

RETURN 0
