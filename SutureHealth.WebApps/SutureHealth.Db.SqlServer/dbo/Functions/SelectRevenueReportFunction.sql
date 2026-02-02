-- =============================================
CREATE FUNCTION [dbo].[SelectRevenueReportFunction] 
(	
	@UserID int,
	@SignerFacilityId int = 0, 
	@Signer int = 0,
	@PatientId int = 0,
	@EffectiveStartDate varchar(50) = '' ,
	@EffectiveEndDate varchar(50) = '' ,
	@ServiceStartDate datetime = '',
	@ServiceEndDate datetime = ''
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT

			R.Id as 'FormId',
			SF.NickName as 'Practice',
			Sum(Rates.Rate) Over() as RevenueSum,
			SUM(Rates.RelativeValueUnit) Over() as RVUSum,
			CASE WHEN 
					SF.FacilityNPI IS NULL  OR  SF.FacilityNPI = '' THEN 'unavailable'
				ELSE
					SF.FacilityNPI
			END as 'PracticeNPI',

			SU.FirstName as 'ProviderFirstName',
			SU.LastName as 'ProviderLastName',
			SU.Suffix as 'ProviderSuffix',
			SU.PrimCredential as 'ProviderCredential',
			SU.UserNPI as 'ProviderNPI',
			P.LastName AS PatientLastName,
			P.FirstName AS PatientFirstName,
			P.Suffix AS PatientSuffix,
			CONVERT(varchar(10),P.DOB,101) AS PatientDOB,
			CASE WHEN 
					P.LastSSN IS NULL THEN 'unavailable'
				ELSE 'xxx-xx-' + P.LastSSN
			END AS 'Last4SSN',
			Medicaid  = CASE WHEN MedicaidIns.CarrierId IS NOT NULL
								THEN CAST(1 as bit)  
								ELSE CAST(0 as bit)
							END,
			Medicare = CASE WHEN MedicareIns.CarrierId IS NOT NULL
								THEN CAST(1 as bit)
								ELSE CAST(0 as bit)
							END,
			MedicareAdvantage = CASE WHEN MedicareAdvantageIns.CarrierId IS NOT NULL
									THEN CAST(1 as bit)
									ELSE CAST(0 as bit)
								END,
			PrivateInsurance = CASE WHEN PrivateIns.CarrierId IS NOT NULL
								THEN CAST(1 as bit)
								ELSE CAST(0 as bit)
							END,
			SelfPay = CASE WHEN SelfPayIns.CarrierId IS NOT NULL
								THEN CAST(1 as bit)
								ELSE CAST(0 as bit)
							END,

			SubmF.[Name] as [ReferringProvider],
			CASE WHEN SubmF.FacilityNPI IS NULL OR SubmF.FacilityNPI = '' THEN 'unavailable' ELSE SubmF.FacilityNPI END  as [ReferringProviderNPI],
			CASE WHEN SubmF.MedicareNumber IS NULL OR SubmF.MedicareNumber = '' THEN 'unavailable' ELSE SubmF.MedicareNumber END  as [ReferringProviderMedicare],
			TP.TemplateDisplayName AS [DocumentType],
			'Office' as 'PlaceOfService',
			R.StDate AS [ServiceDate],
			ISNULL(R.EffDate, R.StartOfCare) AS EffectiveDate,
			DxCodes.ICD9Code as 'DiagnosisCode',
			Rates.BillingCode as 'BillingCode',
			Rates.Rate as 'Revenue',
			Rates.RelativeValueUnit as RVU,
			COUNT(*) OVER () AS Total
	
	FROM [$(SutureSignWeb)].[dbo].Requests R

		INNER JOIN [$(SutureSignWeb)].[dbo].Users_Facilities UF ON R.Signer = UF.Id 
			AND UF.Active = 1 AND R.[Disabled] = 0 and R.[Status] = 1
			AND (@SignerFacilityId = 0 OR @SignerFacilityId = UF.FacilityId) 
			AND (@Signer = 0 OR @Signer = UF.UserId)
			AND ((@ServiceStartDate = '' OR CONVERT(DATETIME, @ServiceStartDate) <= R.StDate)
			AND (@ServiceEndDate = ''  OR CONVERT(DATETIME, @ServiceEndDate) > R.StDate))
			AND ((@EffectiveStartDate = '' OR CONVERT(DATE, @EffectiveStartDate) <= ISNULL(R.EffDate, R.StartOfCare)) 
			AND (@EffectiveEndDate = ''  OR CONVERT(DATE, @EffectiveEndDate) > ISNULL(R.EffDate, R.StartOfCare)))
		INNER JOIN [$(SutureSignWeb)].[dbo].Tasks SendRequestTasks ON SendRequestTasks.TaskId = R.Rt
		INNER JOIN [$(SutureSignWeb)].[dbo].Patients P ON P.PatientId = R.Patient
			AND (@PatientId = 0 OR @PatientId = P.PatientId)
		INNER JOIN [$(SutureSignWeb)].[dbo].Templates T ON T.TemplateId = R.Template
		INNER JOIN [$(SutureSignWeb)].[dbo].TemplateProperties TP ON TP.TemplatePropertyId = T.TemplatePropertyId
			AND TP.TemplatePropertyId in (1001, 1003)
		INNER JOIN [$(SutureSignWeb)].[dbo].Rates ON Rates.TemplatePropertyId = TP.TemplatePropertyId AND Rates.Active = 1
		INNER JOIN [$(SutureSignWeb)].[dbo].Facilities SF ON UF.FacilityId = SF.FacilityId
		INNER JOIN [$(SutureSignWeb)].[dbo].Users_Facilities UserUF ON UserUF.FacilityId = UF.FacilityId AND UserUF.UserId = @UserID AND UserUF.Active = 1 
		INNER JOIN [$(SutureSignWeb)].[dbo].Users_Facilities SubmUF ON SubmUF.Id = R.Submitter
		INNER JOIN [$(SutureSignWeb)].[dbo].Facilities SubmF on SubmF.FacilityId = SubmUF.FacilityId
		INNER JOIN [$(SutureSignWeb)].[dbo].Users SU ON SU.UserId = UF.UserId
		LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].Users_Managers UMManager ON UMManager.ManagerId = @UserID AND UMManager.UserId = UF.UserId and UMManager.Active = 1 
		LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].Users_Managers UMAssistant ON UMAssistant.UserId = @UserID AND UMAssistant.ManagerId = UF.UserId and UMAssistant.Active = 1
		LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].PatientIns MedicareIns
				ON MedicareIns.PatientId = P.PatientId
					AND MedicareIns.PatientInsId = (Select TOP 1 MC.PatientInsId FROM [$(SutureSignWeb)].[dbo].PatientIns MC where MC.CarrierId = 1 and MC.PatientId = P.PatientId)  --medicare
			LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].PatientIns MedicareAdvantageIns
				ON MedicareAdvantageIns.PatientId = P.PatientId
					AND MedicareAdvantageIns.CarrierId = 2  --medicare advantage
			LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].PatientIns MedicaidIns
				ON MedicaidIns.PatientId = P.PatientId
					AND MedicaidIns.CarrierId = 3  --medicaid
			LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].PatientIns SelfPayIns
				ON SelfPayIns.PatientId = P.PatientId
					AND SelfPayIns.CarrierId = 4  --self
			LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].PatientIns PrivateIns
				ON PrivateIns.PatientId = P.PatientId
					AND PrivateIns.CarrierId = 5  --private
		LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].ICD9Codes DxCodes
				ON R.ICD9CodeId = DxCodes.Id

	WHERE 
		UF.UserId = @UserId
			OR UMManager.ManagerId = @UserId
			OR UMAssistant.UserId = @UserId
			OR UserUF.IsBillingAdmin = 1
)
GO

