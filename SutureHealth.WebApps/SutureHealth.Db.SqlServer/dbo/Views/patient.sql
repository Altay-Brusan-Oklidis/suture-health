CREATE VIEW [dbo].[Patient]
AS
	SELECT p.[PatientId],
		   p.FirstName,
		   p.MiddleName,
		   p.LastName,
		   p.Suffix,
		   p.DOB								[BirthDate],
		   COALESCE(p.Gender, 'U')				[Gender],
		   CAST(COALESCE(p.Active, 0) AS BIT)	[IsActive],
		   p.SSN								[SocialSecurityNumber],
		   p.LastSSN							[SocialSecuritySerialNumber],
		   p.CreateDate							[CreatedAt],
		   p.ChangeDate							[UpdatedAt],
		   p.ChangeBy							[UpdatedBy]
	  FROM [$(SutureSignWeb)].[dbo].[Patients] p WITH (NOLOCK)