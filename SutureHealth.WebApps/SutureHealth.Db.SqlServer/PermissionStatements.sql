SET IDENTITY_INSERT [dbo].[Role] ON;
GO

INSERT INTO [dbo].[Role]
(
		 [RoleId]
		,[Name]
		,[Description]
)
VALUES
(1, 'ApplicationAdministrator', 'A member of the Suture Health Support Team.'),
(2, 'AuthenticatedMember', 'A member that has completed a successful login with their username and password.'),
(3, 'OrganizationAdministrator', 'A member that can create, read, or remove users of a specific organization id.'),
(4,	'Document Sender',''),
(5,	'Document Signer',''),
(6,	'Revenue Reviewer',''),
(7,	'Document Approver',''),
(8,	'Care Team Member',''),
(9,	'Care Team Doctor',''),
(10, 'Care Team Nurse',''),
(11, 'Care Team Assistant', '')

GO

SET IDENTITY_INSERT [dbo].[Role] OFF;
SET IDENTITY_INSERT [dbo].[ActionIdentifier] ON;
GO

INSERT INTO [dbo].[ActionIdentifier]
(
		 [ActionIdentifierId]
		,[DomainObject]
		,[ActionName]
		,[ActionIdentifierKey]
)
VALUES 
(1, 'Member', 'Update Member', 'srn:member:UpdateMembers'),
(2, 'Member', 'Reset Password', 'srn:member:ChangeMembersPassword'),
(3, 'Member', 'Register Member', 'srn:member:RegisterMembers'),
(4, 'Member', 'Create Member', 'srn:member:CreateMembers'),
(5, 'Member', 'Describe Member', 'srn:member:DescribeMembers'),
(6, 'Member', 'Deactivate Member', 'srn:member:DeactivateMembers'),
(7, 'Member', 'Delete Member', 'srn:member:DeleteMembers'),
(8, 'Member', 'Describe Member Identity', 'srn:member:DescribeIdentities'),
(9, 'Member', 'List Members', 'srn:member:ListMembers'),
(10, 'Member','Signin to Application', 'srn:member:SignInMembers'),

(11, 'Organization', 'Update Organization', 'srn:organization:UpdateOrganizations'),
(12, 'Organization', 'Create Organization', 'srn:organization:CreateOrganizations'),
(13, 'Organization', 'Describe Organization', 'srn:organization:DescribeOrganizations'),
(14, 'Organization', 'Deactivate Organization', 'srn:organization:DeactivateOrganizations'),
(15, 'Organization', 'Delete Organization', 'srn:organization:DeleteOrganizations'),
(16, 'Organization', 'List Organizations', 'srn:member:ListOrganizations'),

(17, 'Patient', 'Create Patient', 'srn:patient:CreatePatients'),
(18, 'Patient', 'Describe Patient', 'srn:patient:DescribePatients'),
(19, 'Patient', 'List Patients', 'srn:patient:ListPatients'),
(20, 'Patient', 'Match Patients', 'srn:patient:MatchPatients'),
(21, 'Patient', 'Update Patient', 'srn:patient:UpdatePatients'),

(22, 'Template', 'Create Template', 'srn:template:CreateTemplates'),
(23, 'Template', 'Describe Template', 'srn:template:DescribeTemplates'),
(24, 'Template', 'List Template', 'srn:template:ListTemplates'),
(25, 'Template', 'Update Template', 'srn:template:DescribeTemplates')

GO

SET IDENTITY_INSERT [dbo].[ActionIdentifier] OFF;
GO

INSERT INTO [dbo].[RoleAction]
(
    [RoleId]
   ,[ActionIdentifierId]
)
VALUES
(1,1),
(1,2),
(1,3),
(1,4),
(1,5),
(1,6),
(1,7),
(1,8),
(1,9),
(1,11),
(1,12),
(1,13),
(1,14),
(1,15),
(1,16),
(1,17),
(1,18),
(1,19),
(1,20),
(1,21),
(1,22),
(1,23),
(1,24),
(1,25),

(2,1),
(2,2),
(2,3),
(2,5),
(2,10),

(3,1),
(3,2),
(3,3),
(3,4),
(3,5),
(3,6),
(3,7),
(3,9),
(3,11),
(3,12),
(3,13),
(3,14),
(3,15),
(3,16)
GO
