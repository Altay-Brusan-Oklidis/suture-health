--CREATE VIEW [dbo].[MemberPermission]
--AS 

---- DECLARE @MemberId INT

---- SET @MemberId = 3001042       -- Staff
---- SET @MemberId = 3000003       -- Sender
---- SET @MemberId = 3000013       -- Signer + Organization Administrator
---- SET @MemberId = 3000006       -- Assistant
---- SET @MemberId = 3000009       -- Collaborator
---- SET @MemberId = 3000020       -- Organization Administrator
---- SET @MemberId = 3003118       -- Application Administrator

---- SELECT * FROM Member WHERE MemberId = @MemberId

---- SELECT *
----   FROM 
---- (  

---- manual entries
--SELECT mps.MemberId
--     , mps.Effect
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , mps.ResourceIdentifierKey
--  FROM dbo.MemberPermissionStatement mps
-- INNER JOIN dbo.ActionIdentifier ai ON mps.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE mps.DeletedAt IS NULL

-- UNION ALL

--/* 
-- *  MEMBER PERMISSIONS
-- */

---- entries preventing deactivation or deletion of oneself
--SELECT m.MemberId
--     , CAST(0 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , CONCAT('srn:member:*:', m.memberId)
--  FROM dbo.Member m
-- INNER JOIN dbo.ActionIdentifier ai ON ai.ActionIdentifierId IN (6,7) 
-- WHERE ai.DomainObject = 'Member'
 
-- UNION ALL
 
---- entries allowing for login and modification of one's own account
--SELECT m.MemberId
--     , CAST(1 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , CONCAT('srn:member:*:', m.memberId)
--  FROM dbo.Member m
-- INNER JOIN dbo.Role r ON r.Id = 2
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE ai.DomainObject = 'Member'
--   AND NOT ai.ActionIdentifierId IN (6,7)
 
-- UNION ALL

---- APPLICATION ADMINISTRATORS
--SELECT m.MemberId
--     , CAST(1 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , 'srn:member:*:*'
--  FROM dbo.Member m
-- INNER JOIN dbo.Role r ON r.Id = 1
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE m.MemberTypeId = 2016
--   AND ai.DomainObject = 'Member'
   

-- UNION ALL

---- ORGANIZATION ADMINISTRATORS 
--SELECT m.MemberId
--     , CAST(1 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , CONCAT('srn:member:', om.OrganizationId, ':*')
--  FROM dbo.Member m
-- INNER JOIN dbo.OrganizationMember om ON m.MemberId = om.MemberId
-- INNER JOIN dbo.Role r ON r.Id = 3
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
--  WHERE om.IsActive = 1
--   AND om.IsAdministrator = 1
--   AND om.OrganizationId > 0
--   AND COALESCE(om.EffectiveAt, '1/1/1900') < GETUTCDATE()
--   AND COALESCE(om.DeactivatedAt, '12/31/2999') > GETUTCDATE()
--   AND ai.DomainObject = 'Member'

--/* 
-- *  ORGANIZATION PERMISSIONS
-- */

-- UNION ALL

---- APPLICATION ADMINISTRATORS
--SELECT m.MemberId
--     , CAST(1 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , 'srn:organization:*:*'
--  FROM dbo.Member m
-- INNER JOIN dbo.Role r ON r.Id = 1
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE m.MemberTypeId = 2016
--   AND ai.DomainObject = 'Organization'

-- UNION ALL

---- ORGANIZATION ADMINISTRATORS 
---- prevent primary organization of member from being deactivated or deleted
--SELECT m.MemberId
--     , CAST(0 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , CONCAT('srn:organization:', om.OrganizationId, ':*')
--  FROM dbo.Member m
-- INNER JOIN dbo.OrganizationMember om ON m.MemberId = om.MemberId
-- INNER JOIN dbo.Role r ON r.Id = 3
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE om.IsActive = 1
--   AND om.IsAdministrator = 1
--   AND om.IsPrimary = 1
--   AND om.OrganizationId > 0
--   AND COALESCE(om.EffectiveAt, '1/1/1900') < GETUTCDATE()
--   AND COALESCE(om.DeactivatedAt, '12/31/2999') > GETUTCDATE()
--   AND ai.DomainObject = 'Organization'
--   AND ai.ActionIdentifierId IN (14, 15)
-- UNION ALL
--SELECT m.MemberId
--     , CAST(1 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , CONCAT('srn:organization:', om.OrganizationId, ':*')
--  FROM dbo.Member m
-- INNER JOIN dbo.OrganizationMember om ON m.MemberId = om.MemberId
-- INNER JOIN dbo.Role r ON r.Id = 3
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE om.IsActive = 1
--   AND om.IsAdministrator = 1
--   AND om.OrganizationId > 0
--   AND COALESCE(om.EffectiveAt, '1/1/1900') < GETUTCDATE()
--   AND COALESCE(om.DeactivatedAt, '12/31/2999') > GETUTCDATE()
--   AND ai.DomainObject = 'Organization'

--/* 
-- *  PATIENT PERMISSIONS
-- */

-- UNION ALL

---- APPLICATION ADMINISTRATORS
--SELECT m.MemberId
--     , CAST(1 as TINYINT)
--     , ai.ActionIdentifierId
--     , ai.ActionIdentifierKey
--     , 'srn:patient:*:*'
--  FROM dbo.Member m
-- INNER JOIN dbo.Role r ON r.Id = 1
-- INNER JOIN dbo.RoleAction ra ON r.Id = ra.RoleId
-- INNER JOIN dbo.ActionIdentifier ai ON ra.ActionIdentifierId = ai.ActionIdentifierId
-- WHERE m.MemberTypeId = 2016
--   AND ai.DomainObject = 'Patient'

---- ) a
---- WHERE a.MemberId = @MemberId
   