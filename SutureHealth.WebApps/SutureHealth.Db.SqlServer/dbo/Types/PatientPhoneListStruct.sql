CREATE TYPE [dbo].[PatientPhoneList] AS TABLE
(
	[PatientId] INT,
    [Type] NCHAR(50), 
    [Value] NCHAR(50), 
    [IsActive] BIT,     
    [IsPrimary] BIT
)