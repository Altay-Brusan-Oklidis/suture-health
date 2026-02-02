CREATE PROCEDURE [dbo].[UpdatePatientSocialSecurity]
  @PatientId int,
  @Last4Ssn			varchar(4),
  @SSN				varchar(9),
  @ChangeBy INT
AS
BEGIN
UPDATE Patient
	   Set 
		 [SocialSecuritySerialNumber] =  @Last4Ssn,
		 [SocialSecurityNumber] =  @SSN,
		 [UpdatedBy] = @ChangeBy
WHERE PatientId = @PatientId
END
