Declare @PatientId int = 1011718

Select '' AS 'Patients 	-->', * from Patients with (NOLOCK) where PatientId = @PatientId
Select '' AS 'PatientsH 	-->', * from PatientsH with (NOLOCK) where PatientId = @PatientId
Select '' AS 'PatientIns 	-->', * from PatientIns with (NOLOCK) where PatientId = @PatientId
Select '' AS 'PatientInsH 	-->', * from PatientInsH with (NOLOCK) where PatientId = @PatientId
Select '' AS 'PatientAddress 	-->', * from PatientAddress with (NOLOCK) where PatientId = @PatientId
Select '' AS 'PatientAddressH 	-->', * from PatientAddressH with (NOLOCK) where PatientId = @PatientId
Select '' AS 'Facilities_Patients  -->', F.FacilityId, F.[Name], FP.* from Facilities_Patients FP
	with (NOLOCK) JOIN Facilities F with (NOLOCK) ON FP.FacilityId = F.FacilityId
	Where PatientId = @PatientId


