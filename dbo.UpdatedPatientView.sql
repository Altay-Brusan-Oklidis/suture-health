USE [SutureSignApi-CI]
GO

/****** Object:  View [dbo].[Patient]    Script Date: 4/14/2023 2:42:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER VIEW [dbo].[Patient]
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
		   p.AdmissionStatus					[AdmissionStatus],
		   p.CreateDate							[CreatedAt],
		   p.ChangeDate							[UpdatedAt],
		   p.ChangeBy							[UpdatedBy]
	  FROM [SutureSignWeb-CI].[dbo].[Patients] p WITH (NOLOCK)
GO


