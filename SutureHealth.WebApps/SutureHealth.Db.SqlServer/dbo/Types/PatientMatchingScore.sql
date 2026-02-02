CREATE TYPE [dbo].[PatientMatchingScore] AS TABLE
(
	PatientId	INT, 
	Score		FLOAT,
	[Override]	BIT
)
