CREATE PROCEDURE [dbo].[CreatePatient]
(
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
    @Phones PatientPhoneList READONLY,
    @NewPatientId int OUTPUT
)
AS
BEGIN
    DECLARE @RC INT,
            @AssociationExists BIT;
    EXECUTE @RC = [$(SutureSignWeb)].[dbo].[spAddPatient] 
       @FirstName                   = @FirstName
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
      ,@PatientId                   = -1
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
      ,@PrivateGrpNumber            = @PrivateGrpNumber
      ,@NewPatientId                = @NewPatientId         OUTPUT
      ,@AssociationExists           = @AssociationExists    OUTPUT;

    DECLARE @PhoneInfoChanged bit

    IF @NewPatientId > 0
        BEGIN         
	       INSERT 
           INTO [$(SutureSignWeb)].[dbo].[PatientPhone] ([PatientId],[Type],[Value],[IsActive],[IsPrimary])
           SELECT @NewPatientId, [ph].[Type], [ph].[Value], [ph].[IsActive], [ph].[IsPrimary] 
           FROM @Phones ph
        END
END