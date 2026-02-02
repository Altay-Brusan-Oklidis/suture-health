CREATE TABLE [dbo].[ReportEligibility]
(
	[ReportEligibilityId]	INT			NOT NULL IDENTITY(1, 1) CONSTRAINT PK_ReportEligibility PRIMARY KEY CLUSTERED,
	[ReportTypeId]			SMALLINT	NOT NULL				CONSTRAINT FK_ReportEligibility_ReportTypeId FOREIGN KEY REFERENCES dbo.ReportType([ReportTypeId]),
	[Channel]				TINYINT		NOT	NULL,
	[CompanyId]				INT			NULL, 
	[OrganizationTypeId]	INT			NULL, 
	[OrganizationId]		INT			NULL, 
	[MemberTypeId]			INT			NULL, 
	[MemberId]				INT			NULL, 
	[CanSign]				BIT			NULL,
	[IsCollaborator]		BIT			NULL
)