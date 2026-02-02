CREATE PROCEDURE [dbo].[SelectOrganizationIdsInPatientScopeForSender]
	@MemberId INT,
	@OrganizationId INT = NULL
AS
BEGIN
	SELECT DISTINCT all_o.OrganizationId
	FROM OrganizationMember om WITH (NOLOCK)
		INNER JOIN Organization o WITH (NOLOCK) ON om.OrganizationId = o.OrganizationId AND o.IsActive = 1
		LEFT JOIN Organization company_o WITH (NOLOCK) ON o.CompanyId = company_o.OrganizationId AND company_o.IsActive = 1
		LEFT JOIN OrganizationSetting company_os WITH (NOLOCK) ON company_o.OrganizationId = company_os.ParentId AND company_os.[Key] = 'ShareCompanyPatients' AND company_os.ItemBool = 1 AND company_os.IsActive = 1
		CROSS APPLY
			(
				SELECT om.OrganizationId
				UNION ALL
				SELECT related_o.OrganizationId
				FROM Organization related_o WITH (NOLOCK)
				WHERE company_os.SettingId IS NOT NULL AND related_o.CompanyId = company_o.OrganizationId AND related_o.IsActive = 1 AND related_o.OrganizationId <> om.OrganizationId
			) all_o
	WHERE om.MemberId = @MemberId AND (@OrganizationId IS NULL OR om.OrganizationId = @OrganizationId) AND om.IsActive = 1
END