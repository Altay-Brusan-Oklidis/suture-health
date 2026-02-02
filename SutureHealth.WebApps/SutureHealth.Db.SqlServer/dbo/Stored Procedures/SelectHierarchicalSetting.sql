CREATE PROCEDURE [dbo].[SelectHierarchicalSetting]
	@MemberId INT,
	@OrganizationId INT = NULL,
	@Key VARCHAR(200)
AS
BEGIN
	SELECT
		CASE
			WHEN [ms].SettingId IS NOT NULL THEN [ms].[Key]
			WHEN [os].SettingId IS NOT NULL THEN [os].[Key]
			WHEN [cs].SettingId IS NOT NULL THEN [cs].[Key]
			ELSE [as].[Key]
		END AS [Key],
		CASE
			WHEN [ms].SettingId IS NOT NULL THEN [ms].IsActive
			WHEN [os].SettingId IS NOT NULL THEN [os].IsActive
			WHEN [cs].SettingId IS NOT NULL THEN [cs].IsActive
			ELSE [as].IsActive
		END AS IsActive,
		CASE
			WHEN [ms].SettingId IS NOT NULL THEN [ms].ItemBool
			WHEN [os].SettingId IS NOT NULL THEN [os].ItemBool
			WHEN [cs].SettingId IS NOT NULL THEN [cs].ItemBool
			ELSE [as].ItemBool
		END AS ItemBool,
		CASE
			WHEN [ms].SettingId IS NOT NULL THEN [ms].ItemInt
			WHEN [os].SettingId IS NOT NULL THEN [os].ItemInt
			WHEN [cs].SettingId IS NOT NULL THEN [cs].ItemInt
			ELSE [as].ItemInt
		END AS ItemInt,
		CASE
			WHEN [ms].SettingId IS NOT NULL THEN [ms].ItemString
			WHEN [os].SettingId IS NOT NULL THEN [os].ItemString
			WHEN [cs].SettingId IS NOT NULL THEN [cs].ItemString
			ELSE [as].ItemString
		END AS ItemString,
		CASE
			WHEN [ms].SettingId IS NOT NULL THEN [ms].ItemType
			WHEN [os].SettingId IS NOT NULL THEN [os].ItemType
			WHEN [cs].SettingId IS NOT NULL THEN [cs].ItemType
			ELSE [as].ItemType
		END AS ItemType
	FROM
		(
			SELECT TOP 1 ms.SettingId, ms.ParentId, ms.[Key], ms.IsActive, ms.ItemBool, ms.ItemInt, ms.ItemString, ms.ItemType
			FROM dbo.Member m
				LEFT JOIN dbo.MemberSetting ms ON m.MemberId = ms.ParentId AND ms.[Key] = @Key AND ms.IsActive = 1
			WHERE m.MemberId = @MemberId
		) [ms]
		CROSS JOIN
		(
			SELECT TOP 1 os.SettingId, os.ParentId, os.[Key], os.IsActive, os.ItemBool, os.ItemInt, os.ItemString, os.ItemType
			FROM dbo.Member m
				LEFT JOIN dbo.OrganizationSetting os ON os.ParentId = COALESCE(@OrganizationId, m.PrimaryOrganizationId) AND os.[Key] = @Key AND os.IsActive = 1
			WHERE m.MemberId = @MemberId
		) [os]
		CROSS JOIN
		(
			SELECT TOP 1 [cs].SettingId, [cs].ParentId, [cs].[Key], [cs].IsActive, [cs].ItemBool, [cs].ItemInt, [cs].ItemString, [cs].ItemType
			FROM dbo.Member m
				LEFT JOIN dbo.Organization o ON o.OrganizationId = COALESCE(@OrganizationId, m.PrimaryOrganizationId)
				LEFT JOIN dbo.OrganizationSetting cs ON o.CompanyId = cs.ParentId AND cs.[Key] = @Key AND cs.IsActive = 1
			WHERE m.MemberId = @MemberId
		) [cs]
		CROSS JOIN
		(
			SELECT TOP 1 [as].SettingId, [as].ParentId, [as].[Key], [as].IsActive, [as].ItemBool, [as].ItemInt, [as].ItemString, [as].ItemType
			FROM dbo.Member m
				LEFT JOIN dbo.ApplicationSetting [as] ON [as].IsActive = 1 AND [as].[Key] = @Key
			WHERE m.MemberId = @MemberId
		) [as]
	WHERE
		COALESCE([ms].SettingId, [os].SettingId, [cs].SettingId, [as].SettingId) IS NOT NULL
END