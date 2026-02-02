CREATE VIEW [dbo].[OrganizationImage]
AS 
	SELECT FacilityImageId [OrganizationImageId],
		   FacilityId      [OrganizationId],
		   IsPrimary       [IsPrimary],
		   UploadDate      [UploadDate],
		   Active		   [Active]
	
	FROM [$(SutureSignWeb)].dbo.FacilityImage
