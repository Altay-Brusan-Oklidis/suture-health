CREATE PROCEDURE [dbo].[GetTasksByRequestId]
	@FormId int,
	@PageNumber int
AS
BEGIN
	SET NOCOUNT ON
	SELECT  Tasks.TaskId,
		Tasks.FormId,
		Tasks.UserId,
		Tasks.FacilityId,
		Tasks.ActionId,
		Tasks.TemplateId,
		Tasks.PatientId,
		Tasks.SubmittedBy,
		Tasks.CreateDate,
		Tasks.SubmittedByFacility,	
		Tasks.SurrogateSubmitterOrgId,
		Tasks.EffectiveDate,
		Tasks.StartOfCare,
		Tasks.ICD9CodeId,
		UU.FirstName,
		UU.MiddleName,
		UU.LastName,
		UU.PrimCredential AS Suffix,
		SU.FirstName AS SubmitterFirstName,
		SU.MiddleName AS SubmitterMiddleName,
		SU.LastName AS SubmitterLastName,
		SU.PrimCredential AS SubmitterSuffix,
		Facilities.Name AS OrganizationName,
		TemplateProperties.ServiceCategory,
		TemplateProperties.ServiceCategoryDisplay,
		TemplateProperties.TemplateDisplayName,
		TemplateProperties.TemplateShortName
FROM [$(SutureSignWeb)].[dbo].Tasks
	LEFT JOIN [$(SutureSignWeb)].[dbo].Users AS UU ON Tasks.UserId = UU.UserId
	LEFT JOIN [$(SutureSignWeb)].[dbo].Users AS SU ON Tasks.SubmittedBy = SU.UserId
	LEFT JOIN [$(SutureSignWeb)].[dbo].Templates ON Tasks.TemplateId = Templates.TemplateId
	LEFT JOIN [$(SutureSignWeb)].[dbo].TemplateProperties ON Templates.TemplatePropertyId = TemplateProperties.TemplatePropertyId
	LEFT JOIN [$(SutureSignWeb)].[dbo].Facilities ON Tasks.SubmittedByFacility = Facilities.FacilityId
	WHERE Tasks.FormId = @FormId			
	ORDER BY Tasks.TaskId DESC
	OFFSET (@PageNumber) * 50 ROWS
	FETCH NEXT 50 ROWS ONLY
END
