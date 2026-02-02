CREATE PROCEDURE [dbo].[UpdatePatient]
(
    @PatientId int,
    @FirstName varchar(50),
    @MiddleName varchar(50),
    @MaidenName varchar(50),
    @LastName varchar(50),
    @Suffix varchar(4),
    @Last4Ssn varchar(4),
    @SSN varchar(9),
    @DOB date,
    @Gender char(1),
    @Address1 varchar(150),
    @Address2 varchar(150),
    @City varchar(150),
    @State varchar(2),
    @Zip varchar(9),
    @FacilityMrn varchar(50),
    @FacilityId int,
    @ChangeBy int,
    @IsMedicare bit,
    @IsMedicareAdvantage bit,
    @IsMedicAid bit,
    @IsSelf bit,
    @IsPrivate bit,
    @IsMedicarePrimary bit,
    @IsMedicareAdvantagePrimary bit,
    @IsMedicAidPrimary bit,
    @IsSelfPrimary bit,
    @IsPrivatePrimary bit,
    @MedicareNum varchar(11),
    @MedicareMBI varchar(11),
    @MedicAidNum varchar(50),
    @MedicareAdvantageNum varchar(50),
    @PrivateNum varchar(50),
    @MedicAidState varchar(2),
    @MedicareGrpNumber varchar(50),
    @MedicareAdvGrpNumber varchar(50),
    @MedicaidGrpNumber varchar(50),
    @PrivateGrpNumber varchar(50),
    @Phones PatientPhoneList READONLY
)
AS
BEGIN
    DECLARE @RC INT;
    EXECUTE @RC = [$(SutureSignWeb)].[dbo].[spUpdatePatient] 
       @PatientId                   = @PatientId
      ,@FirstName                   = @FirstName
      ,@MiddleName                  = @MiddleName
      ,@MaidenName                  = @MaidenName
      ,@LastName                    = @LastName
      ,@Suffix                      = @Suffix
      ,@Last4Ssn                    = @Last4Ssn
      ,@SSN                         = @SSN
      ,@DOB                         = @DOB
      ,@Gender                      = @Gender
      ,@Address1                    = @Address1
      ,@Address2                    = @Address2
      ,@City                        = @City
      ,@State                       = @State
      ,@Zip                         = @Zip
      ,@FacilityMrn                 = @FacilityMrn
      ,@FacilityId                  = @FacilityId
      ,@ChangeBy                    = @ChangeBy
      ,@MatchPatientLogID           = -1
      ,@IsMedicare                  = @IsMedicare
      ,@IsMedicareAdvantage         = @IsMedicareAdvantage
      ,@IsMedicAid                  = @IsMedicAid
      ,@IsSelf                      = @IsSelf
      ,@IsPrivate                   = @IsPrivate
      ,@IsMedicarePrimary           = @IsMedicarePrimary
      ,@IsMedicareAdvantagePrimary  = @IsMedicareAdvantagePrimary
      ,@IsMedicAidPrimary           = @IsMedicAidPrimary
      ,@IsSelfPrimary               = @IsSelfPrimary
      ,@IsPrivatePrimary            = @IsPrivatePrimary
      ,@MedicareNum                 = @MedicareNum
      ,@MedicareMBI                 = @MedicareMBI
      ,@MedicAidNum                 = @MedicAidNum
      ,@MedicareAdvantageNum        = @MedicareAdvantageNum
      ,@PrivateNum                  = @PrivateNum
      ,@MedicAidState               = @MedicAidState
      ,@MedicareGrpNumber           = @MedicaidGrpNumber
      ,@MedicareAdvGrpNumber        = @MedicareAdvGrpNumber
      ,@MedicaidGrpNumber           = @MedicaidGrpNumber
      ,@PrivateGrpNumber            = @PrivateGrpNumber;


-- Update the previous primary phone to set IsPrimary to 0
UPDATE [$(SutureSignWeb)].[dbo].PatientPhone
SET [IsPrimary] = 0,
    [ChangeDate] = GETDATE()
FROM [$(SutureSignWeb)].[dbo].PatientPhone
WHERE [PatientId] = @PatientId

UPDATE [$(SutureSignWeb)].[dbo].PatientPhone
SET  [Value] = [@Phones].[Value],
	 [IsActive] = [@Phones].[IsActive],
	 [IsPrimary] = [@Phones].[IsPrimary],
	 [ChangeDate] = GETDATE()
FROM [$(SutureSignWeb)].[dbo].PatientPhone
	 INNER JOIN @Phones
	 ON [PatientPhone].[PatientId] = [@Phones].[PatientId]
     WHERE  [PatientPhone].[Type] = [@Phones].[Type] AND
	         ([PatientPhone].[Value] != [@Phones].[Value] OR [PatientPhone].[IsPrimary] != [@Phones].[IsPrimary])

-- Delete records where @Phones has null or empty values
DELETE PP
FROM [$(SutureSignWeb)].[dbo].PatientPhone AS PP
LEFT JOIN @Phones AS P ON PP.[PatientId] = P.[PatientId] AND PP.[Type] = P.[Type]
WHERE (P.[Value] IS NULL OR P.[Value] = '');

-- Insert new records if they don't exist
 INSERT INTO [$(SutureSignWeb)].[dbo].PatientPhone (PatientId, Type, Value, IsActive, IsPrimary, ChangeDate)
 SELECT [@Phones].[PatientId], [@Phones].[Type], [@Phones].[Value], [@Phones].[IsActive],[@Phones].[IsPrimary], GETDATE()
 FROM @Phones
 LEFT JOIN [$(SutureSignWeb)].[dbo].PatientPhone AS ExistingPhone
 ON ExistingPhone.PatientId = @PatientId AND ExistingPhone.Type = [@Phones].[Type]
 WHERE ExistingPhone.PatientId IS NULL;

END