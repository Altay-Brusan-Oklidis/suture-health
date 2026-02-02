/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
-- Remove deprecated HCHB sprocs and tables.
-- Remove HCHB sprocs and tables from dbo schema.

DECLARE @Schema NVARCHAR(150);
DECLARE @DataBase NVARCHAR(150);
DECLARE @SQL NVARCHAR(MAX);

SET @Schema = '$(HCHBSchema)';
SET @Schema = LTRIM(RTRIM(@Schema));

SET @DataBase = '$(SutureSignWeb)';
SET @DataBase = LTRIM(RTRIM(@DataBase));

DECLARE @DropViews NVARCHAR(MAX);
DECLARE @DropProcs NVARCHAR(MAX);

SET @DropViews =N'
   
	   -- Drop outdated views

       IF OBJECT_ID(''[dbo].[HCHBObservationResult]'') IS NOT NULL
       DROP VIEW [dbo].[HCHBObservationResult];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HCHBObservationResult]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HCHBObservationResult];


       IF OBJECT_ID(''[dbo].[HchbPatientTransaction]'') IS NOT NULL
       DROP VIEW [dbo].[HchbPatientTransaction];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HchbPatientTransaction]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HchbPatientTransaction];';

EXEC sp_executesql @DropViews;

SET @DropViews =N'

       -- Drop old views and clean up dbo schema

       IF OBJECT_ID(''[dbo].[HCHB_Patient]'') IS NOT NULL
       DROP VIEW [dbo].[HCHB_Patient];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HCHB_Patient]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HCHB_Patient];

       IF OBJECT_ID(''[dbo].[HCHB_Templates]'') IS NOT NULL
       DROP VIEW [dbo].[HCHB_Templates];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HCHB_Templates]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HCHB_Templates];

       IF OBJECT_ID(''[dbo].[HL7MessageLog]'') IS NOT NULL
       DROP VIEW [dbo].[HL7MessageLog];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HL7MessageLog]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HL7MessageLog];
      
       IF OBJECT_ID(''[dbo].[Transactions]'') IS NOT NULL
       DROP VIEW [dbo].[Transactions];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[Transactions]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[Transactions];

       
       IF OBJECT_ID(''[dbo].[Branch]'') IS NOT NULL
       DROP VIEW [dbo].[Branch];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[Branch]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[Branch];

       

';

EXEC sp_executesql @DropViews;

SET @DropProcs =N'
        IF OBJECT_ID(''[dbo].[LogHL7Message]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[LogHL7Message];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[LogHL7Message]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[LogHL7Message];
        
        IF OBJECT_ID(''[dbo].[GetSignerPrimaryFacility]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[GetSignerPrimaryFacility];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[GetSignerPrimaryFacility]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[GetSignerPrimaryFacility];'

EXEC sp_executesql @DropProcs;

SET @DropViews =N'
   
	   -- Drop outdated views from all schema

       IF OBJECT_ID(''[dbo].[HCHBObservationResult]'') IS NOT NULL
       DROP VIEW [dbo].[HCHBObservationResult];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HCHBObservationResult]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HCHBObservationResult];


       IF OBJECT_ID(''[dbo].[HchbPatientTransaction]'') IS NOT NULL
       DROP VIEW [dbo].[HchbPatientTransaction];

       IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[HchbPatientTransaction]'') IS NOT NULL
       DROP VIEW '+ QUOTENAME(@Schema) +'.[HchbPatientTransaction];
       
       -- Drop views from dbo schema

       IF OBJECT_ID(''[dbo].[HCHB_Patient]'') IS NOT NULL
       DROP VIEW [dbo].[HCHB_Patient];


       IF OBJECT_ID(''[dbo].[HCHB_Templates]'') IS NOT NULL
       DROP VIEW [dbo].[HCHB_Templates];


       IF OBJECT_ID(''[dbo].[HL7MessageLog]'') IS NOT NULL
       DROP VIEW [dbo].[HL7MessageLog];

       IF OBJECT_ID(''[dbo].[Branch]'') IS NOT NULL
       DROP VIEW [dbo].[Branch];

';

EXEC sp_executesql @DropViews;

SET @DropProcs =N'

		-- Drop outdated procedures from all schemas

        IF OBJECT_ID(''[dbo].[CreateHCHBObservationResult]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[CreateHCHBObservationResult];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[CreateHCHBObservationResult]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[CreateHCHBObservationResult];


        IF OBJECT_ID(''[dbo].[CreateHCHBPatientTransaction]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[CreateHCHBPatientTransaction];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[CreateHCHBPatientTransaction]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[CreateHCHBPatientTransaction];
        
        IF OBJECT_ID(''[dbo].[UpdateHCHBObservationResult]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[UpdateHCHBObservationResult];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[UpdateHCHBObservationResult]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[UpdateHCHBObservationResult];


';

EXEC sp_executesql @DropProcs;

SET @DropProcs =N'

        -- Drop procedures from dbo schema
        
        IF OBJECT_ID(''[dbo].[CreateHCHBPatient]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[CreateHCHBPatient];
        
        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[CreateHCHBPatient]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[CreateHCHBPatient];

        IF OBJECT_ID(''[dbo].[UpdateHCHBPatient]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[UpdateHCHBPatient];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[UpdateHCHBPatient]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[UpdateHCHBPatient];

        IF OBJECT_ID(''[dbo].[GetHchbPatientStatusByPatientId]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[GetHchbPatientStatusByPatientId];
        
        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[GetHchbPatientStatusByPatientId]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[GetHchbPatientStatusByPatientId];

        IF OBJECT_ID(''[dbo].[GetPatientIdByHCHB]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[GetPatientIdByHCHB];
        
        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[GetPatientIdByHCHB]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[GetPatientIdByHCHB];
';

EXEC sp_executesql @DropProcs;

SET @DropProcs =N'
        IF OBJECT_ID(''[dbo].[LogHL7Message]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[LogHL7Message];

        IF OBJECT_ID(''[dbo].[LogProcessedMessage]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[LogProcessedMessage];
        
        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[LogProcessedMessage]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[LogProcessedMessage];

        
        IF OBJECT_ID(''[dbo].[LogReason]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[LogReason];
            
        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[LogReason]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[LogReason];

        ---------------------------------------------------------------------
        IF OBJECT_ID(''[dbo].[AddTemplateHash]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[AddTemplateHash];
        
        IF OBJECT_ID(''[dbo].[GetTemplateHash]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[GetTemplateHash];
        ----------------------------------------------------------------------
        
        IF OBJECT_ID(''[dbo].[GetFacilityIdByBranchCode]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[GetFacilityIdByBranchCode];

        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[GetFacilityIdByBranchCode]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[GetFacilityIdByBranchCode];

        IF OBJECT_ID(''[dbo].[GetSignerPrimaryFacility]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[GetSignerPrimaryFacility];

        ------------------------------------------------
        IF OBJECT_ID(''[dbo].[CreateTransaction]'', ''P'') IS NOT NULL
            DROP PROCEDURE [dbo].[CreateTransaction];
        IF OBJECT_ID('''+ QUOTENAME(@Schema) +'.[CreateTransaction]'', ''P'') IS NOT NULL
            DROP PROCEDURE '+ QUOTENAME(@Schema) +'.[CreateTransaction];

';

EXEC sp_executesql @DropProcs;
