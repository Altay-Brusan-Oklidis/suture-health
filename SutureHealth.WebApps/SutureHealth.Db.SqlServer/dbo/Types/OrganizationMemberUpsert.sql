CREATE TYPE [dbo].[OrganizationMemberUpsert] AS TABLE
(
	OrganizationId INT NOT NULL,
	IsAdministrator BIT NOT NULL,
	IsBillingAdministrator BIT NOT NULL,
	IsPrimary BIT NOT NULL,
	IsActive BIT NOT NULL
)
