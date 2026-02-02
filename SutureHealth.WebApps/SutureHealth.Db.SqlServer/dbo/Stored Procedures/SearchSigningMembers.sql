CREATE PROCEDURE [dbo].[SearchSigningMembers]
	@SearchText VARCHAR(255),
	@Count INT = 10,
	@OrganizationStateOrProvince VARCHAR(2) = NULL
AS
BEGIN
	DECLARE @Members TABLE (MemberId INT NOT NULL);

	IF (LEN(TRIM(COALESCE(@SearchText, ''))) <= 1)
	BEGIN
		SELECT * FROM Member WITH (NOLOCK)
		WHERE 0 = 1;

		RETURN;
	END

	SET @Count = COALESCE(@Count, 10);

	INSERT INTO @Members
	SELECT DISTINCT a.UserId
	FROM [$(SutureSignWeb)].dbo.Users a WITH (NOLOCK)
		INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities d WITH (NOLOCK) ON a.UserId = d.UserId
		INNER JOIN [$(SutureSignWeb)].dbo.Facilities_Locations e WITH (NOLOCK) ON d.FacilityId = e.FacilityId
		INNER JOIN [$(SutureSignWeb)].dbo.Locations f WITH (NOLOCK) ON e.LocationId = f.LocationId
	WHERE a.CanSign = 1
		AND (a.LastName LIKE '%' + @SearchText + '%'
				OR a.FirstName LIKE '%' + @SearchText + '%'
				OR a.UserNPI LIKE @SearchText + '%'
				OR a.FirstName + ' ' + a.LastName +
				CASE WHEN a.Suffix IS NOT NULL AND LTRIM(RTRIM(a.Suffix)) <> '' THEN
					', ' + a.Suffix
				ELSE
					''
				END LIKE @SearchText + '%'
				OR a.LastName + ', ' + a.FirstName +
				CASE WHEN a.Suffix IS NOT NULL AND LTRIM(RTRIM(a.Suffix)) <> '' THEN
					' ' + a.Suffix
				ELSE
					''
				END LIKE @SearchText + '%')
		AND (@OrganizationStateOrProvince IS NULL OR (@OrganizationStateOrProvince IS NOT NULL AND f.[State] = @OrganizationStateOrProvince))
		AND a.Active = 1
		AND d.Active = 1
		AND d.EffectiveDate <= GETDATE()
		AND (d.ExpirationDate > GETDATE() OR d.ExpirationDate IS NULL)
		AND e.Active = 1
		AND e.EffectiveDate <= GETDATE()
		AND f.Active = 1
		AND a.[Suspend] IS NULL
		AND d.[Suspend] IS NULL;

	SELECT TOP (@Count) m.*
	FROM Member m WITH (NOLOCK)
		INNER JOIN @Members m_ids ON m.MemberId = m_ids.MemberId
	ORDER BY m.LastName DESC;
END