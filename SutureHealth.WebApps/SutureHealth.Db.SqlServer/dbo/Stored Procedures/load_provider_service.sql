
-- =============================================
-- Author:		  kkilburn
-- Create date: 2020-12-22
-- Description:	loads the services
-- =============================================
CREATE PROCEDURE [dbo].[load_provider_service]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT Statements.
	SET NOCOUNT ON;
	
	INSERT 
	  INTO [stage].[ProviderService]([ServiceId],[ProviderId])
    SELECT ServiceId
		  ,ProviderId
      FROM [import].ProviderService
	 
END
