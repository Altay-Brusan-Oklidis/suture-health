CREATE PROCEDURE [dbo].[LogPatientMatch]
    @SubmittedFirstName         VARCHAR(50),
    @SubmittedMiddleName        VARCHAR(50),
    @SubmittedLastName          VARCHAR(50),
    @SubmittedSuffix            VARCHAR(50),
    @SubmittedDOB               DATE,
    @SubmittedGender            CHAR(1),
    @SubmittedSSN               VARCHAR(9),
    @SubmittedLastSSN           VARCHAR(4),
    @SubmittedMedicaid#         VARCHAR(50),
    @SubmittedMedicaidState     VARCHAR(2),
    @SubmittedFacilityMRN       VARCHAR(50),
    @SubmittedAddress1          VARCHAR(150),
    @SubmittedAddress2          VARCHAR(150),
    @SubmittedCity              VARCHAR(150),
    @SubmittedState             VARCHAR(2),
    @SubmittedZip               VARCHAR(9),
    @SubmittedHomePhone         VARCHAR(15),
    @SubmittedWorkPhone         VARCHAR(15),
    @SubmittedMobilePhone       VARCHAR(15),
    @SubmittedOtherPhone        VARCHAR(15),
    @SubmittedPrimaryPhone      VARCHAR(15),
    @MatchAlgorithmID           INT,
    @MatchedPatient             BIT,
    @NeedsReview                BIT, 
    @ManuallyMatched            BIT, 
    @ManuallyMatchedBy          INT, 
    @ManuallyMatchedOn          DATETIME, 
    @MultiplePatientsMatched    BIT,
    @SimilarPatientHigh         BIT,
    @SimilarPatientLow          BIT,
    @ProcessingStartTime        DATETIME,
    @ProcessingEndTime          DATETIME,
    @RecordsEvaluated           INT,
    @SubmittedBy                INT,
    @SubmittedByFacility        INT,
    @SubmittedMedicareMBI       VARCHAR(50),
    @SubmittedSource            VARCHAR(150), 
    @SourceDescription          VARCHAR(MAX),
    @IsSelfPay                  BIT, 
    @IsPrivateInsurance         BIT , 
    @IsMedicareAdvantage        BIT,
    @MatchingResults            PatientMatchingScore READONLY
AS
     INSERT 
       INTO [$(SutureSignWeb)].[dbo].[MatchPatientLog]
           ([SubmittedFirstName]
           ,[SubmittedMiddleName]
           ,[SubmittedLastName]
           ,[SubmittedSuffix]
           ,[SubmittedDOB]
           ,[SubmittedGender]
           ,[SubmittedSSN]
           ,[SubmittedLastSSN]
           ,[SubmittedMedicare#]
           ,[SubmittedMedicaid#]
           ,[SubmittedMedicaidState]
           ,[SubmittedFacilityMRN]
           ,[SubmittedAddress1]
           ,[SubmittedAddress2]
           ,[SubmittedCity]
           ,[SubmittedState]
           ,[SubmittedZip]
           ,[SubmittedHomePhone]
           ,[SubmittedWorkPhone]
           ,[SubmittedMobilePhone]
           ,[SubmittedOtherPhone]
           ,[SubmittedPrimaryPhone]
           ,[MatchAlgorithmID]
           ,[MatchedPatient]
           ,[NeedsReview]
           ,[ManuallyMatched] 
           ,[ManuallyMatchedBy]
           ,[ManuallyMatchedOn]
           ,[MultiplePatientsMatched]
           ,[SimilarPatientHigh]
           ,[SimilarPatientLow]
           ,[ProcessingStartTime]
           ,[ProcessingEndTime]
           ,[RecordsEvaluated]
           ,[CreateDate]
           ,[SubmittedBy]
           ,[SubmittedByFacility]
           ,[SubmittedMedicareMBI]
           ,[SubmittedSource]
           ,[SourceDescription]
           ,[IsSelfPay] 
           ,[IsPrivateInsurance] 
           ,[IsMedicareAdvantage])
     VALUES
           (@SubmittedFirstName,
            @SubmittedMiddleName,
            @SubmittedLastName,
            @SubmittedSuffix,
            @SubmittedDOB,
            @SubmittedGender,
            @SubmittedSSN,
            @SubmittedLastSSN,
            NULL,
            @SubmittedMedicaid#,
            @SubmittedMedicaidState,
            @SubmittedFacilityMRN,
            @SubmittedAddress1,
            @SubmittedAddress2,
            @SubmittedCity,
            @SubmittedState,
            @SubmittedZip,
            @SubmittedHomePhone,
            @SubmittedWorkPhone,
            @SubmittedMobilePhone,
            @SubmittedOtherPhone,
            @SubmittedPrimaryPhone,
            @MatchAlgorithmID,
            @MatchedPatient,
            @NeedsReview, 
            @ManuallyMatched, 
            @ManuallyMatchedBy, 
            @ManuallyMatchedOn, 
            @MultiplePatientsMatched,
            @SimilarPatientHigh,
            @SimilarPatientLow,
            @ProcessingStartTime,
            @ProcessingEndTime,
            @RecordsEvaluated,
            GETUTCDATE(),
            @SubmittedBy,
            @SubmittedByFacility,
            @SubmittedMedicareMBI,
            @SubmittedSource,
            @SourceDescription,
            @IsSelfPay, 
            @IsPrivateInsurance, 
            @IsMedicareAdvantage)

   DECLARE @MatchPatientLogId INT;
   DECLARE @LoggingThreshold  INT;

       SET @MatchPatientLogId = SCOPE_IDENTITY();

    SELECT @LoggingThreshold = LoggingThreshold
	  FROM [$(SutureSignWeb)].dbo.SystemSettings ss
	 INNER JOIN [$(SutureSignWeb)].dbo.MatchAlgorithms ma ON ss.ItemInt = ma.MatchAlgorithmID
	 WHERE ss.Setting = 'PatientMatchAlgorithm'

    INSERT 
      INTO [$(SutureSignWeb)].dbo.MatchPatientOutcome
    SELECT @MatchPatientLogID, mr.PatientID, mr.Score, 0, 0, GETDATE() 
      FROM @MatchingResults mr
	 WHERE Score >= @LoggingThreshold
        OR [Override] = 1
	 ORDER BY Score
GO