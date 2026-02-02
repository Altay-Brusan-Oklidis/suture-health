CREATE PROCEDURE [dbo].[SelectRevenueMetricsReport]
	 @UserID int,
     @SignerFacilityId int = 0, 
     @Signer int = 0
AS
BEGIN
DECLARE
@OneYearAgo DATETIME = DATEADD(YEAR, -1, GETDATE()),
@ThirtyDaysAgo DATETIME = DATEADD(DAY, -30, GETDATE())

	SELECT
		SUM(Rates.Rate) as AllTimeRevenue,
		SUM(Rates.RelativeValueUnit) as AllTimeRVU,
		SUM(CASE WHEN R.StDate >@ThirtyDaysAgo  THEN Rates.Rate END) as Last30DaysRevenue,
		SUM(CASE WHEN R.StDate >@ThirtyDaysAgo  THEN Rates.RelativeValueUnit END) as Last30DaysRVU,
		SUM(CASE WHEN R.StDate >@OneYearAgo  THEN Rates.Rate END) as LastYearRevenue,
		SUM(CASE WHEN R.StDate >@OneYearAgo  THEN Rates.RelativeValueUnit END) as LastYearRVU

	FROM [$(SutureSignWeb)].[dbo].Requests R
			INNER JOIN [$(SutureSignWeb)].[dbo].Users_Facilities UF ON R.Signer = UF.Id
				AND UF.Active = 1 AND R.[Disabled] = 0 and R.[Status] = 1
				AND (@SignerFacilityId = 0 OR @SignerFacilityId = UF.FacilityId) 
				AND (@Signer = 0 OR @Signer = UF.UserId)
			INNER JOIN [$(SutureSignWeb)].[dbo].Templates T ON T.TemplateId = R.Template
			INNER JOIN [$(SutureSignWeb)].[dbo].TemplateProperties TP ON TP.TemplatePropertyId = T.TemplatePropertyId
				AND TP.TemplatePropertyId in (1001, 1003)
			INNER JOIN [$(SutureSignWeb)].[dbo].Rates ON Rates.TemplatePropertyId = TP.TemplatePropertyId AND Rates.Active = 1
			INNER JOIN [$(SutureSignWeb)].[dbo].Users_Facilities UserUF ON UserUF.FacilityId = UF.FacilityId AND UserUF.UserId = @UserID AND UserUF.Active = 1
			LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].Users_Managers UMManager ON UMManager.ManagerId = @UserID AND UMManager.UserId = UF.UserId and UMManager.Active = 1
			LEFT OUTER JOIN [$(SutureSignWeb)].[dbo].Users_Managers UMAssistant ON UMAssistant.UserId = @UserID AND UMAssistant.ManagerId = UF.UserId and UMAssistant.Active = 1
	
	WHERE UF.UserId = @UserId
			OR UMManager.ManagerId = @UserId
			OR UMAssistant.UserId = @UserId
			OR UserUF.IsBillingAdmin = 1

END
GO


