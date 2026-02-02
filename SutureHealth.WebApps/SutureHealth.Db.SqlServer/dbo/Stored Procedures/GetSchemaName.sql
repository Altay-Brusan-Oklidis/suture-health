CREATE PROCEDURE [dbo].[GetSchemaName]
	@ApplicationId nvarchar(100),
	@FacilityId nvarchar(100),
	@Schema nvarchar(100) OUT
AS

BEGIN

	SELECT @Schema=[Schema] FROM [$(SutureSignWeb)].[dbo].[Clients] 
	WHERE [ApplicationId] = @ApplicationId and [FacilityId] = @FacilityId;

END
