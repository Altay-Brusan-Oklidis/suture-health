CREATE VIEW [dbo].[PatientPhone]
	AS SELECT 
	ph.[PatientPhoneId],
    ph.[PatientId],
    ph.[Type],
    ph.[Value],
    ph.[IsActive],    
    ph.[IsPrimary],
    ph.[CreateDate],
    ph.[ChangeDate]
	FROM [$(SutureSignWeb)].[dbo].[PatientPhone] ph
