
DECLARE @PatientLastName varchar(50), @SelectedMatch int, @Submittedby int,
@MatchPatientLogID int, @SupportUserId int, @MatchScoreLow int
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		--Get Patient Match Info				
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------									
--SET @SupportUserId = 3000001  (doesn't work yet)		--3000001: General Support		--3022418: Braden		--3026735: Lové 
----------------------------

----*************************************************************************************************************************************************************
		
	----STEP 1 - SEARCH PATIENT MATCH LOG BY LAST NAME----


	SET @PatientLastName 		= 'Brusan'			--Choose either this OR @Submittedby 
	--SET @Submittedby			= 3000000			--Choose either this OR @PatientLastName (Can be either UserId, FacilityId, or CompanyId)
	--SET @MatchPatientLogID	= 335314			--Then you can override with this
	
----*************************************************************************************************************************************************************


	----STEP 2: UPDATE EXISTING OR ADD NEW PATIENT --** UNCOMMENT & EXECUTE ONLY THE SECTION BELOW **--


--------********************	UNCOMMENT THIS ROWDown to Ln 46		********************

DECLARE @PatientIdToUpdate int, @PatientMatchLogIdToUpdate int, @SelectedMatch int


SET @PatientIdToUpdate  			= 	1012325		--UPDATE PATIENT: SET TO EXISTING PATIENT ID ... or ... CREATE NEW PATIENT: SET TO -1

SET @PatientMatchLogIdToUpdate  	= 16313		--SET TO THE MATCH PATIENT LOG ID THAT YOU WANT TO USE TO CREATE OR UPDATE THE PATIENT


SELECT * FROM MatchPatientLog WHERE MatchPatientLogID = @PatientMatchLogIdToUpdate
IF @PatientIdToUpdate != -1 BEGIN SELECT 'PATIENTS TABLE BEFORE UPDATE' SELECT * FROM Patients WHERE PatientID = @PatientIdToUpdate END
EXEC spAddPatientFromMatchPatientLog @PatientMatchLogIdToUpdate, @PatientIdToUpdate
IF @PatientIdToUpdate = -1 SET @PatientIdToUpdate = (SELECT TOP 1 PatientId FROM Patients ORDER BY PatientId DESC)

SELECT 'The patient you attempted to add will now show in your search in SutureSign' AS 'NEW PATIENT RECORDS'
SELECT A.PatientId, A.FirstName, A.MiddleName, A.LastName, A.MaidenName, A.DOB, A.Gender, A.SSN, A.LastSSN AS 'Last4SSN', E.CarrierName AS 'InsCarrier', D.PolicyNumber, B.FacilityMRN, F.Address1, F.Address2, F.City, F.[State], F.Zip, C.Name AS 'Facility', C.FacilityId, A.CreateDate AS 'PtCreate',B.ChangeDate AS 'PtChng', B.ChangeBy AS 'Added By'
FROM Patients A, Facilities_Patients B, Facilities C, PatientAddress F left outer join PatientIns D ON @SelectedMatch = D.PatientId left outer join InsuranceCarriers E ON D.CarrierId = E.CarrierId WHERE A.PatientId = B.PatientId and B.FacilityId = C.FacilityId and A.Patientid = F.PatientID and A.PatientID = @PatientIdToUpdate and A.Active = 1 ORDER BY B.ChangeDate DESC
SELECT 'PATIENT HISTORY LOGS'	SELECT B.ChangeDate AS 'Patient Add/Change', A.PatientId, A.FirstName, A.MiddleName, A.LastName, A.MaidenName, A.DOB, A.Gender, A.SSN, A.LastSSN AS 'Last4SSN', E.CarrierName AS 'InsCarrier', D.PolicyNumber, B.FacilityMRN, F.Address1, F.Address2, F.City, F.[State], F.Zip, C.Name AS 'Facility', C.FacilityId, A.CreateDate AS 'PtCreate', B.ChangeBy
FROM PatientsH A, Facilities_Patients B, Facilities C, PatientAddressH F left outer join PatientInsH D ON @SelectedMatch = D.PatientId left outer join InsuranceCarriers E ON D.CarrierId = E.CarrierId WHERE A.PatientId = B.PatientId and B.FacilityId = C.FacilityId and A.Patientid = F.PatientID and A.HistoryType = F.HistoryType and A.PatientID = @PatientIdToUpdate and A.Active = 1 ORDER BY B.ChangeDate DESC

--------********************	 UNCOMMENT THIS ROW Up to Ln 25		********************



----*************************************************************************************************************************************************************
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

				----------DO NOT EDIT BELOW THIS----------



--Select top 2 ML.* from MatchPatientLog ML INNER JOIN PatientIns I ON ML.SubmittedMedicareMBI = I.PolicyNumber and I.PlanId = 4 order by createdate desc

IF @MatchPatientLogID is NULL and @PatientLastName is NULL and @Submittedby is NULL
	BEGIN SET @MatchPatientLogID = (SELECT TOP 1 MatchPatientLogID FROM MatchPatientLog ORDER BY MatchPatientLogID DESC)
	END ELSE

IF @MatchPatientLogID is NULL and @Submittedby is NULL and @PatientLastName is NOT NULL
	BEGIN SET @MatchPatientLogID = (SELECT TOP 1 MatchPatientLogID FROM MatchPatientLog WHERE SubmittedLastName = @PatientLastName ORDER BY MatchPatientLogID DESC)
	END ELSE

IF @MatchPatientLogID is NULL and @PatientLastName is NULL and @Submittedby is NOT NULL
	BEGIN SET @MatchPatientLogID = (SELECT TOP 1 MatchPatientLogID FROM MatchPatientLog WHERE (SubmittedBy = @Submittedby or SubmittedByFacility = @Submittedby) or (SubmittedByFacility in (SELECT FacilityId FROM Facilities WHERE CompanyId = @Submittedby)) ORDER BY MatchPatientLogID DESC)
	END

----------"Recent Submissions" Statement----------------------------------------------------------------------------------------------------------------------------------

SELECT 'SUBMISSIONS matching on Last Name or SubmittedBy' AS ' '
SELECT TOP 200 MPL.MatchPatientLogID, MPL.SubmittedFirstName, MPL.SubmittedMiddleName, MPL.SubmittedLastName, MPL.SubmittedMaidenName, MPL.SubmittedDOB, MPL.SubmittedGender, MPL.SubmittedSSN, MPL.SubmittedLastSSN, MPL.SubmittedMedicare#, MPL.SubmittedMedicareMBI, MPL.SubmittedMedicaid#, MPL.SubmittedMedicaidState, MPL.SubmittedFacilityMRN, 
	MPL.SubmittedAddress1, MPL.SubmittedAddress2, MPL.SubmittedCity, MPL.SubmittedState, MPL.SubmittedZip, MPL.MatchAlgorithmID, MPL.MatchedPatient, MPL.MultiplePatientsMatched, MPL.SimilarPatientHigh, MPL.SimilarPatientLow, MPL.ProcessingStartTime, MPL.ProcessingEndTime, MPL.RecordsEvaluated, MPL.CreateDate, MPL.SubmittedByFacility, F.Name, MPL.SubmittedBy, U.SigningName
FROM MatchPatientLog MPL 
	join Facilities F WITH (NOLOCK) ON F.FacilityId = MPL.SubmittedByFacility
	join Users U WITH (NOLOCK) ON U.UserId = MPL.SubmittedBy
WHERE (MPL.SubmittedLastName = @PatientLastName and @Submittedby is null) 
	or (@PatientLastName is NULL and (MPL.Submittedby = @Submittedby or MPL.SubmittedByFacility = @Submittedby or MPL.SubmittedByFacility in (SELECT FacilityId FROM Facilities WHERE CompanyId = @Submittedby)))
ORDER BY MatchPatientLogID DESC


----------"Single Submission being Evaluated" Statement----------------------------------------------------------------------------------------------------------

SET @SelectedMatch = (SELECT PatientId FROM MatchPatientOutcome WITH (NOLOCK) WHERE MatchPatientOutcomeID = (SELECT Top 1 MatchPatientOutcomeID FROM MatchPatientOutcome WITH (NOLOCK) WHERE MatchPatientLogID = @MatchPatientLogID Order By MatchScore desc, MatchPatientOutcomeID desc))

SELECT 'Submission Being Evaluated ---' AS ' '
SELECT TOP 1 CASE WHEN (MPO.MatchScore >= 57 and MPO.MatchScore < 100) THEN ('High Risk Similar / Must Call: ' + (CAST(MPO.MatchScore AS varchar(2)))) WHEN (MPO.MatchScore < 0) THEN ( 'NEW (Added by system): ' + (CAST(MPO.MatchScore AS varchar(2)))) WHEN (MPO.MatchScore > 100) THEN ( 'Matched (Added By System): ' + (CAST(MPO.MatchScore AS varchar(2)))) ELSE ('Low Risk Similar / Verify Info Alert: ' + (CAST(MPO.MatchScore AS varchar(2)))) END AS ' Match Status / Popup: [Score] '
	,MPL.MatchPatientLogID, MPL.SubmittedFirstName AS 'Sub_FirstName', MPL.SubmittedMiddleName AS 'Sub_MiddleName', MPL.SubmittedLastName AS 'Sub_LastName', MPL.SubmittedMaidenName AS 'Sub_MaidenName', MPL.SubmittedDOB AS 'Sub_DOB', MPL.SubmittedGender AS 'Sub_Gender', MPL.SubmittedSSN AS 'Sub_SSN', MPL.SubmittedLastSSN AS 'Sub_Last4SSN'
	,CASE WHEN MPL.SubmittedMedicareMBI is NOT NULL THEN ('MedicareMBI: ' + (CAST(MPL.SubmittedMedicareMBI AS varchar(MAX)))) WHEN MPL.SubmittedMedicare# is NOT NULL THEN ('Medicare: ' + (CAST(MPL.SubmittedMedicare# AS varchar(MAX)))) WHEN MPL.SubmittedMedicaid# is NOT NULL THEN ('Medicaid: ' + (CAST(MPL.SubmittedMedicaid# AS varchar(MAX)))) ELSE '' END AS '	Sub_Insurance'
	--,MPL.SubmittedMedicareMBI AS 'Sub_MedicareMBI#', MPL.SubmittedMedicaid# AS 'Sub_Medicaid#'--, MPL.SubmittedMedicaidState AS 'Sub_MedicaidState'
	,MPL.SubmittedFacilityMRN, MPL.SubmittedAddress1 AS 'Sub_Address1', MPL.SubmittedAddress2 AS 'Sub_Address2', MPL.SubmittedCity AS 'Sub_City', MPL.SubmittedState AS 'Sub_State', MPL.SubmittedZip AS 'Sub_Zip','----------------' AS ' ', F.FacilityId, (F.Name+' - '+L.City), MPL.SubmittedBy, (U.FirstName + ' ' + U.LastName) AS 'Submitted By' 
	,MPL.CreateDate, L.City, L.[State], FC.Value AS 'Phone', MPL.MatchAlgorithmID, MPL.MatchedPatient, MPL.MultiplePatientsMatched, MPL.SimilarPatientHigh, MPL.SimilarPatientLow, MPL.ProcessingStartTime, MPL.ProcessingEndTime, MPL.RecordsEvaluated
FROM MatchPatientOutcome MPO 
	JOIN MatchPatientLog MPL WITH (NOLOCK) ON MPL.MatchPatientLogID = MPO.MatchPatientLogID
	left outer join Facilities F WITH (NOLOCK) ON F.FacilityId = MPL.SubmittedByFacility
	left outer join Facilities_Contacts FC WITH (NOLOCK) ON FC.FacilityId = F.FacilityId and FC.[Primary] = 1 and FC.[Type] = 'phone' 
	left outer join Facilities_Locations FL WITH (NOLOCK) ON FL.FacilityId = F.FacilityId
	left outer join Locations L WITH (NOLOCK) ON L.LocationId = FL.LocationId 
	left outer join Users U WITH (NOLOCK) ON U.UserId = MPL.SubmittedBy
WHERE MPL.MatchPatientLogId = @MatchPatientLogID and MPO.Patientid = @SelectedMatch
ORDER BY MPO.MatchPatientOutcomeID DESC

--Select @SelectedMatch as 'PatientId Test'

Declare @IDMatches Table (PatientId Int, SSN varchar(9), MBI varchar(20), Medicaid varchar(20), MedicaidState varchar(2))
Insert Into @IDMatches
SELECT P.PatientId, P.SSN, CASE WHEN I.PlanId = 4 THEN I.PolicyNumber ELSE NULL END, CASE WHEN I.CarrierId = 3 THEN I.PolicyNumber ELSE NULL END, CASE WHEN I.CarrierId = 3 THEN I.State ELSE NULL END
	FROM Patients P  WITH (NOLOCK)
		LEFT OUTER JOIN PatientIns I WITH (NOLOCK) ON I.PatientId = P.PatientId
WHERE (P.SSN = (Select SubmittedSSN From MatchPatientLog WITH (NOLOCK) Where MatchPatientLogID = @MatchPatientLogID))
	OR (I.PlanId = 4 and I.PolicyNumber = (Select SubmittedMedicareMBI From MatchPatientLog WITH (NOLOCK) Where MatchPatientLogID = @MatchPatientLogID))
	OR (I.CarrierId = 3 and I.PolicyNumber = (Select SubmittedMedicaid# From MatchPatientLog WITH (NOLOCK) Where MatchPatientLogID = @MatchPatientLogID) and I.State = (Select SubmittedMedicaidState From MatchPatientLog WITH (NOLOCK) Where MatchPatientLogID = @MatchPatientLogID))



----------"Existing Patients and Scores" Statement--------------------------------------------------------------------------------------------------------------------------------------------------

--Select @SelectedMatch
--Select * from @IDMatches

SELECT 'EXISTING PATIENT'AS 'EXISTING Patient(s)',((CAST(MPO.MatchScore AS varchar(3)))) AS 'Match Score', CASE WHEN @SelectedMatch = MPO.PatientID THEN 'TOP MATCH' ELSE NULL END as 'Match Position',
CASE WHEN P.SSN in (Select SSN From @IDMatches) THEN 'SSN' WHEN I.PlanId = 4 and I.PolicyNumber in (Select MBI From @IDMatches) THEN 'MBI' WHEN I.CarrierId = 3 and I.PolicyNumber in (Select Medicaid From @IDMatches) and I.State in (Select MedicaidState From @IDMatches) THEN 'Medicaid' ELSE NULL END as 'ID Match',P.PatientId AS '-------PatientId-------',
P.Active, P.FirstName AS '---FirstName----', P.MiddleName AS '----MiddleName---', P.LastName AS '----LastName---', P.MaidenName AS '----MaidenName---', P.DOB AS '----DOB-----', P.Gender AS '---Gender----', P.SSN AS '---SSN----', P.LastSSN AS '----Last4SSN---'
	,Case WHEN (I.CarrierId != I.PlanId) and I.PolicyNumber is not NULL THEN (IP.PlanName +': '+I.PolicyNumber) WHEN (I.CarrierId != I.PlanId) and I.PolicyNumber is NULL THEN IP.PlanName
		  WHEN ((I.CarrierId = I.PlanId) or (I.PlanId is null)) and I.PolicyNumber is not NULL THEN (IC.CarrierName +': '+I.PolicyNumber) WHEN ((I.CarrierId = I.PlanId)or(I.PlanId is null)) and I.PolicyNumber is NULL THEN IC.CarrierName ELSE '' END AS '		PatientIns(s)'
	--, (IC.CarrierName + ': ' + (CAST(I.PolicyNumber AS varchar(MAX)))) AS '----------Insurance Info-----------' --, I.PolicyNumber AS '---PolicyNumber---' --, I.PolicyNumber AS '---Insurance#---'
	, FP.FacilityMRN AS '-------FacilityMRN------', PA.Address1 AS '---Address1----', PA.Address2 AS '---Address2----', PA.City AS '---City----', PA.[State] AS '---State----', PA.Zip AS '---Zip----','----------------' AS ' ', F.FacilityId, (F.Name+' - '+L.City) AS 'Facility', FP.ChangeBy AS 'Added By', P.CreateDate AS 'PtCreate', FP.ChangeDate AS 'PtChng'
	, CASE WHEN P.SSN in (Select SSN From @IDMatches) THEN 1 WHEN I.PlanId = 4 and I.PolicyNumber in (Select MBI From @IDMatches) THEN 1 WHEN I.CarrierId = 3 and I.PolicyNumber in (Select Medicaid From @IDMatches) and I.State in (Select MedicaidState From @IDMatches) THEN 1 ELSE 0 END as 'IDMatchSort'
FROM Patients P WITH (NOLOCK)
	left outer join Facilities_Patients FP WITH (NOLOCK) ON P.PatientId = FP.PatientId 
	left outer join Facilities F WITH (NOLOCK) ON F.FacilityId = FP.FacilityId 
	left outer join Facilities_Locations FL WITH (NOLOCK) ON FL.FacilityId = F.FacilityId
	left outer join Locations L WITH (NOLOCK) ON L.LocationId = FL.LocationId 
	left outer join PatientAddress PA WITH (NOLOCK) ON PA.PatientId = FP.PatientId
	left outer join PatientIns I WITH (NOLOCK) ON I.PatientId = PA.PatientId 
	left outer join InsuranceCarriers IC WITH (NOLOCK) ON I.CarrierId = IC.CarrierId	
	left outer join InsurancePlans [IP] WITH (NOLOCK) ON [IP].PlanId = I.PlanId
	left outer join MatchPatientOutcome MPO WITH (NOLOCK) ON MPO.PatientID = P.PatientId and (MatchPatientLogID = @MatchPatientLogID)
	Left outer join @IDMatches ID ON ID.PatientId = P.PatientId
WHERE MPO.PatientID = @SelectedMatch
		OR MPO.PatientID in (Select top 5 MPO2.PatientID FROM MatchPatientOutcome MPO2 Where MPO2.MatchPatientLogID = MPO.MatchPatientLogID Order By MPO2.MatchScore desc)
		OR P.PatientId in (Select ID.PatientId FROM @IDMatches ID)
-- and I.Active = 1 and P.Active = 1
ORDER BY 'IDMatchSort' desc, 'ID Match' desc, MPO.MatchScore DESC, FP.ChangeDate DESC


SELECT 'PATIENT HISTORY LOGS'
SELECT A.ChangeDate AS 'Patient Add/Change', A.FirstName, A.MiddleName, A.LastName, A.MaidenName, A.DOB, A.Gender, A.SSN, A.LastSSN AS 'Last4SSN'--, E.CarrierName AS 'InsCarrier', D.PolicyNumber
	, B.FacilityMRN, F.Address1, F.Address2, F.City, F.[State], F.Zip, C.Name AS 'Facility', C.FacilityId, A.HistoryType, A.CreateDate AS 'PtCreate', B.ChangeBy
FROM PatientsH A WITH (NOLOCK) 
	inner join MatchPatientOutcome MPO WITH (NOLOCK) ON MPO.PatientID = A.PatientId and (MatchPatientLogID = @MatchPatientLogID and MPO.MatchScore >= @MatchScoreLow)
	JOIN Facilities_Patients B WITH (NOLOCK) ON A.PatientId = B.PatientId
	JOIN Facilities C WITH (NOLOCK) ON B.FacilityId = C.FacilityId
	JOIN PatientAddressH F WITH (NOLOCK) ON A.Patientid = F.PatientID and A.HistoryType = F.HistoryType
	right outer join PatientInsH D WITH (NOLOCK) ON @SelectedMatch = D.PatientId 
	right outer join InsuranceCarriers E WITH (NOLOCK) ON D.CarrierId = E.CarrierId and D.Active = 1
WHERE A.Active = 1 --and A.PatientID = @SelectedMatch 
ORDER BY A.PatientId, A.ChangeDate DESC



SELECT 'Other Potential Matches Below:' AS ' ' 
SELECT MPO.*, '----------------------------------------' AS ' ', P.PatientId, P.FirstName, P.LastName, P.DOB, P.SSN 
FROM MatchPatientOutcome MPO 
	left outer join Patients P WITH (NOLOCK) ON P.PatientId = MPO.PatientID 
WHERE MPO.MatchPatientLogID = @MatchPatientLogID-- and MPO.MatchScore <= @MatchScoreLow
ORDER BY MPO.MatchScore DESC

--SELECT 'MATCH PATIENT OUTCOME THRESHOLDS & ENTRIES'
--SELECT * FROM MatchAlgorithms WHERE MatchAlgorithmID = (SELECT ItemInt FROM SystemSettings WHERE SETTING = 'PatientMatchAlgorithm')

--SELECT DATEDIFF(ms,ProcessingStartTime,ProcessingEndTime)AS 'Processing Time (ms)' FROM MatchPatientLog WHERE MatchPatientLogID = @MatchPatientLogID

--SELECT 'PATIENT MATCH RULES (with point values)'
--SELECT MR.MatchRuleID, AttributeName, [Description], MatchPoints, NonMatchPoints FROM MatchRules MR, MatchAlgorithms_MatchRules MAMR, MatchAlgorithms MA 
--WHERE MA.MatchAlgorithmID = MAMR.MatchAlgorithmID and MAMR.MatchRuleID = MR.MatchRuleID and Ma.MatchAlgorithmID = (SELECT ItemInt FROM SystemSettings WHERE SETTING = 'PatientMatchAlgorithm') ORDER BY MR.MatchPoints DESC




-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------
--SELECT 'Patient History Logs: ' AS ' '
--SELECT (CONVERT(nvarchar(MAX), PIH.ChangeDateH, 23)) AS '---PI Update---', (CONVERT(nvarchar(MAX), FP.ChangeDate, 23)) AS '---FP Update---', PH.PatientId AS 'PatientId', PH.FirstName AS '------FirstName-------', PH.MiddleName AS '------MiddleName-------', PH.LastName AS '------LastName-------', PH.MaidenName AS '------MaidenName-------'
--	----PIH.ChangeDateH AS '---PI Update---', FP.ChangeDate AS '---FP Update---', PH.PatientId AS 'PatientId', PH.FirstName AS '------FirstName-------', PH.MiddleName AS '------MiddleName-------', PH.LastName AS '------LastName-------', PH.MaidenName AS '------MaidenName-------'
--, PH.DOB AS '------DOB-------', PH.Gender AS '-------Gender-------', PH.SSN AS '-------SSN-------', PH.LastSSN AS '------Last4SSN------'
--	, PIH.PolicyNumber AS '----PolicyNumber----'--, IC.CarrierName AS '-------InsCarrier-------'
--	, FP.FacilityMRN, PAH.Address1, PAH.Address2, PAH.City, PAH.[State], PAH.Zip
--	, F.Name AS 'Facility', F.FacilityId, PH.HistoryType, PH.CreateDate AS 'PtCreate', FP.ChangeBy
--FROM Facilities_Patients FP
--	left outer join PatientsH PH
--			left outer join PatientAddressH PAH ON PAH.HistoryType = PH.HistoryType and PH.Patientid = PAH.PatientID 
--	----outer apply (SELECT TOP 1 * FROM PatientInsH WHERE PatientId = @SelectedMatch and Active = 1 ORDER BY ChangeDateH DESC) PIH
--			left outer join PatientInsH PIH 
--				left outer join InsuranceCarriers IC ON PIH.CarrierId = IC.CarrierId
--			  ON @SelectedMatch = PIH.PatientId and PIH.Active = 1
--		 ON PH.PatientId = FP.PatientId and PH.Active = 1 and PH.PatientID = @SelectedMatch 
--	left outer join Facilities F ON F.FacilityId = FP.FacilityId
--WHERE PH.PatientID = @SelectedMatch 
--ORDER BY FP.FacilityId ASC, PIH.ChangeDateH DESC
-----------------------------------------------------------------------------------------------------------