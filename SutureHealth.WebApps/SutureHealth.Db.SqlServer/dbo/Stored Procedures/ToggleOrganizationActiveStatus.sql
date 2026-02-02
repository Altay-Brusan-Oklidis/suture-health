CREATE PROCEDURE [dbo].[ToggleOrganizationActiveStatus]
	@OrganizationId INT,
	@UpdatedByMemberId INT
AS
BEGIN
	IF EXISTS (SELECT 1 FROM [$(SutureSignWeb)].dbo.Facilities WITH (NOLOCK) WHERE Id = @OrganizationId AND Active = 0)
	BEGIN
		UPDATE [$(SutureSignWeb)].dbo.Facilities
		SET Active = 1,
			UpdatedBy = @UpdatedByMemberId,
			DateMod = GETDATE()
		WHERE Id = @OrganizationId;

		SELECT CAST(1 AS BIT) AS IsActive;
	END
	ELSE
	BEGIN
		IF EXISTS (SELECT 1 FROM [$(SutureSignWeb)].dbo.Requests r WITH (NOLOCK)
						INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf WITH (NOLOCK) ON r.Signer = uf.Id AND r.[Status] IS NULL
				   WHERE uf.FacilityId = @OrganizationId)
		BEGIN
			-- One or more signers has requests in-flight; don't change active status of any entities.
			SELECT CAST(1 AS BIT) AS IsActive;
			RETURN;
		END

		UPDATE [$(SutureSignWeb)].dbo.Facilities
		SET Active = 0,
			UpdatedBy = @UpdatedByMemberId,
			DateMod = GETDATE()
		WHERE Id = @OrganizationId;

		UPDATE [$(SutureSignWeb)].dbo.Users_Facilities
		SET Active = 0,
			ActiveUpdate = GETDATE()
		WHERE FacilityId = @OrganizationId AND Active = 1;

		UPDATE u
		SET Active = 0,
			ActiveUpdate = GETDATE(),
			UpdatedDate = GETDATE()
		FROM [$(SutureSignWeb)].dbo.Users u
			INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON u.UserId = uf.UserID
			LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities other_uf ON uf.UserId = other_uf.UserId AND other_uf.FacilityId <> @OrganizationId AND other_uf.Active = 1
		WHERE uf.FacilityId = @OrganizationId AND other_uf.FacilityId IS NULL AND u.Active = 1;

		SELECT CAST(0 AS BIT) AS IsActive;
	END
END