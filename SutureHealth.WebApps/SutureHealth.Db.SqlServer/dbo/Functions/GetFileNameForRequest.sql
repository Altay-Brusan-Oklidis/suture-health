CREATE FUNCTION [dbo].[GetFileNameForRequest]
(
	@LegacyRequestId INT,
	@IsSigner BIT
)
RETURNS VARCHAR(MAX)
AS
BEGIN
	-- NOTE: This function should only be used by dbo.MarkRequestDownloaded for backwards compatibility purposes when generating the 558 task's description.
	--		 It is not and should not be used by the application for determining the actual file name generated.
	DECLARE @FacilityID INT = 0,
			@TemplateId INT = 0,
			@TemplateDisplayName NVARCHAR(200),
			@EffectiveDate AS VARCHAR(10),
			@FacilityDisplayName AS VARCHAR(200),
			@PatientFirstName AS VARCHAR(100),
			@PatientLastName AS VARCHAR(100),
			@PatientDOB AS VARCHAR(10);

	SELECT @FacilityID = UF.FacilityId,
		   @TemplateId = R.Template,
		   @FacilityDisplayName = T.FacilityDisplayName,
		   @TemplateDisplayName = TP.TemplateDisplayName,
		   @EffectiveDate = CONVERT(VARCHAR(10), ISNULL(R.EffDate, R.StartOfCare), 110),
		   @PatientFirstName = P.FirstName,
		   @PatientLastName = P.LastName,
		   @PatientDOB = CONVERT(VARCHAR(10),P.DOB,110)
	FROM [$(SutureSignWeb)].dbo.Requests R WITH (NOLOCK)
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities UF WITH (NOLOCK) ON R.Submitter = UF.Id and R.Id = @LegacyRequestId
		INNER JOIN [$(SutureSignWeb)].dbo.Templates T WITH (NOLOCK) ON R.Template = T.TemplateId
		INNER JOIN [$(SutureSignWeb)].dbo.TemplateProperties TP WITH (NOLOCK) ON T.TemplatePropertyId = TP.TemplatePropertyId
		INNER JOIN [$(SutureSignWeb)].dbo.Patients P WITH (NOLOCK) ON R.Patient = P.PatientId;

	RETURN @PatientLastName + '_' + @PatientFirstName + '_' + 'DOB_' + REPLACE(CAST(@PatientDOB AS VARCHAR(10)), '/', '-') + '__' +
		CASE @IsSigner WHEN 1 THEN @TemplateDisplayName ELSE @FacilityDisplayName END +
		+ '_' + REPLACE(CAST(@EffectiveDate AS VARCHAR(10)), '/', '-') + '_' + CAST(@LegacyRequestId AS VARCHAR(200));
END
