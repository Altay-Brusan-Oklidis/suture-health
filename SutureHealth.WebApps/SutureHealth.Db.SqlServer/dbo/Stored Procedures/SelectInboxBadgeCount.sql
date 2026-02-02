CREATE PROCEDURE [dbo].[SelectInboxBadgeCount]
	@MemberId INT
AS
BEGIN
	DECLARE @MemberCanSign BIT;
	DECLARE @ViewLock BIT;
	DECLARE @ViewForms VARCHAR(200);
	DECLARE @FacilityUserIds TABLE ([FacilityUserId] INT NOT NULL);
	DECLARE @Requests TABLE
	(
		[RequestId] INT NOT NULL,
		[IsApproved] BIT NOT NULL,
		[IsHelpRequested] BIT NOT NULL
	);

	SELECT
		@MemberCanSign = COALESCE(u.CanSign, 0),
		@ViewLock = COALESCE(vl_s.ItemBit, 0),
		@ViewForms = COALESCE(vf_s.ItemVarChar, 'True')
	FROM
		[$(SutureSignWeb)].dbo.Users u WITH (NOLOCK)
			LEFT JOIN [$(SutureSignWeb)].dbo.UserSettings vl_s WITH (NOLOCK) ON u.UserId = vl_s.UserId AND vl_s.Setting = 'signer-view-lock' AND vl_s.Active = 1
			LEFT JOIN [$(SutureSignWeb)].dbo.UserSettings vf_s WITH (NOLOCK) ON u.UserId = vf_s.UserId AND vf_s.Setting = 'ViewFormsByStatus' AND vf_s.Active = 1
	WHERE
		u.UserId = @MemberId;

	IF (@MemberCanSign = 1)
	BEGIN
		INSERT INTO @FacilityUserIds (FacilityUserId)
		SELECT fu.Id
		FROM
			[$(SutureSignWeb)].dbo.Users_Facilities fu WITH (NOLOCK)
				INNER JOIN [$(SutureSignWeb)].dbo.Facilities f WITH (NOLOCK) ON fu.FacilityId = f.FacilityId AND f.Active = 1
		WHERE
			fu.UserId = @MemberId AND fu.Active = 1;
	END
	ELSE
	BEGIN
		INSERT INTO @FacilityUserIds (FacilityUserId)
		SELECT DISTINCT managers_fu.Id
		FROM
			[$(SutureSignWeb)].dbo.Users_Facilities managers_fu WITH (NOLOCK)
				INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities member_fu WITH (NOLOCK) ON member_fu.UserId = @MemberId AND managers_fu.FacilityId = member_fu.FacilityId
				INNER JOIN
				(
					SELECT UserId, ManagerId
					FROM [$(SutureSignWeb)].dbo.Users_Managers WITH (NOLOCK)
					WHERE UserId = @MemberId AND Active = 1
					UNION ALL
					SELECT UserId, ManagerId
					FROM [$(SutureSignWeb)].dbo.Users_Managers WITH (NOLOCK)
					WHERE ManagerId = @MemberId AND Active = 1
				) um ON ((managers_fu.UserId = um.ManagerId AND um.UserId = @MemberId) OR (managers_fu.UserId = um.UserId AND um.ManagerId = @MemberId))
				INNER JOIN [$(SutureSignWeb)].dbo.Facilities f WITH (NOLOCK) ON managers_fu.FacilityId = f.FacilityId AND f.Active = 1
				INNER JOIN [$(SutureSignWeb)].dbo.Users u WITH (NOLOCK) ON managers_fu.UserId = u.UserId AND u.Active = 1
		WHERE
			managers_fu.Active = 1;
	END

	INSERT INTO @Requests (RequestId, IsApproved, IsHelpRequested)
	SELECT DISTINCT
		r.Id,
		CASE WHEN r.[At] IS NOT NULL THEN 1 ELSE 0 END,
		CASE WHEN r.[Ht] IS NOT NULL THEN 1 ELSE 0 END
	FROM
		[$(SutureSignWeb)].dbo.Requests r WITH (NOLOCK)
			INNER JOIN @FacilityUserIds fu_ids ON r.Signer = fu_ids.FacilityUserId
	WHERE
		r.[Status] IS NULL AND r.[Disabled] = 0;


	SELECT COUNT(*) AS [Count]
	FROM @Requests r
	WHERE
		@ViewLock = 0 OR
		(
			@ViewLock = 1 AND
				(PATINDEX('Approved', @ViewForms) = 0 OR r.IsApproved = 1) AND
				(PATINDEX('Unapproved', @ViewForms) = 0 OR r.IsApproved = 0) AND
				(PATINDEX('False', @ViewForms) = 0 OR r.IsHelpRequested = 0)
		);
END