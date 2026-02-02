CREATE VIEW [dbo].[PatientAddress]
AS
	SELECT
		pa.[AddressId]					[PatientAddressId],
		pa.[PatientId],
		pa.[Address1]					[Line1],
		pa.[Address2]					[Line2],
		pa.[City],
		pa.[State]						[StateOrProvince],
		pa.[Zip]						[PostalCode],
		pa.[Country]					[CountryOrRegion],
		pa.[CreateDate],
		pa.[ChangeDate]
	FROM [$(SutureSignWeb)].[dbo].[PatientAddress] pa WITH (NOLOCK)
	WHERE [Active] = 1