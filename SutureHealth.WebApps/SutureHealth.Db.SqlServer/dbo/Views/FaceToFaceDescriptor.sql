CREATE VIEW [dbo].[FaceToFaceDescriptor]
AS
SELECT
	SelectionId									[FaceToFaceDescriptorId],
	Selection									[Description],
	Active										[IsActive],
	ISNULL(SeqNumber, 0)						[SequenceNumber],
	CASE
		WHEN Selection LIKE '--%'
			THEN CAST(1 AS BIT)
		ELSE CAST(0 AS BIT)
	END											[IsPlaceholder],
	CASE SelectionType
		WHEN 'ClinicalReasons_Nursing' THEN 'NursingClinicalReason'
		WHEN 'ClinicalReasons_PT' THEN 'PtClinicalReason'
		WHEN 'ClinicalReasons_Speech' THEN 'SpeechClinicalReason'
		WHEN 'HomeboundReasons' THEN 'HomeboundReason'
		WHEN 'MedicalConditions' THEN 'MedicalCondition'
		ELSE 'Unknown'
	END											[Type]
FROM [$(SutureSignWeb)].dbo.Selections
WHERE SelectionType IN ('ClinicalReasons_Nursing', 'ClinicalReasons_PT', 'ClinicalReasons_Speech', 'HomeboundReasons', 'MedicalConditions');