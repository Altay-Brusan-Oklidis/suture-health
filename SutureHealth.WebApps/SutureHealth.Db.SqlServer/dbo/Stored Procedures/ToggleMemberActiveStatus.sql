CREATE PROCEDURE [dbo].[ToggleMemberActiveStatus]
	@MemberId INT
AS
BEGIN
	DECLARE @IsActive BIT;

	SELECT @IsActive = [Active]
	FROM [$(SutureSignWeb)].dbo.Users WITH (NOLOCK)
	WHERE UserId = @MemberId;

	IF (@IsActive = 1)
	BEGIN
		IF EXISTS	-- Are there any in-flight requests?
		(
			SELECT 1
			FROM [$(SutureSignWeb)].dbo.Requests r WITH (NOLOCK)
				INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK) ON r.Signer = uf.Id
			WHERE uf.UserId = @MemberId AND r.[Status] IS NULL
		) OR EXISTS	-- Are there any organizations with only one administrator?
		(
			SELECT uf.FacilityId, COUNT(*) AS [AdminCount]
			FROM
			(
				SELECT FacilityId
				FROM [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK)
				WHERE uf.UserId = @MemberId AND uf.[Admin] = 1 AND uf.[Active] = 1
			) admin_orgs
				INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK)
					ON admin_orgs.FacilityId = uf.FacilityId
			WHERE uf.[Admin] = 1 AND uf.[Active] = 1
			GROUP BY uf.FacilityId
			HAVING COUNT(*) = 1
		)
		BEGIN
			SELECT CAST(1 AS BIT) AS IsActive;
			RETURN;
		END

		UPDATE [$(SutureSignWeb)].dbo.Users_Facilities
		SET [Active] = 0,
			[Primary] = 0,
			[ActiveUpdate] = GETDATE()
		WHERE UserId = @MemberId;
	END
	ELSE
	BEGIN
		DECLARE @FacilityId INT = NULL;

		SELECT TOP 1 @FacilityId = FacilityId
		FROM [$(SutureSignWeb)].dbo.Users_Facilities WITH (NOLOCK)
		WHERE UserId = @MemberId
		ORDER BY [Active] DESC, [Primary] DESC, [Id] DESC;

		IF (@FacilityId IS NULL)
		BEGIN
			SELECT CAST(0 AS BIT) AS IsActive;
			RETURN;
		END

		UPDATE [$(SutureSignWeb)].dbo.Users_Facilities
		SET [Active] = CASE WHEN FacilityId = @FacilityId THEN 1 ELSE [Active] END,
			[ActiveUpdate] = GETDATE(),
			[Primary] = CASE WHEN FacilityId = @FacilityId THEN 1 ELSE 0 END
		WHERE UserId = @MemberId;
	END

	UPDATE [$(SutureSignWeb)].dbo.Users
	SET [Active] = CASE [Active] WHEN 0 THEN 1 ELSE 0 END,
		[ActiveUpdate] = GETDATE(),
		[UpdatedDate] = GETDATE()
	WHERE UserId = @MemberId;

	SELECT CAST(CASE @IsActive WHEN 0 THEN 1 ELSE 0 END AS BIT) AS [IsActive];
END