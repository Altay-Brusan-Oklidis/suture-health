CREATE PROCEDURE [dbo].[UpdateSutureSignPatient]
	@RequestPatientId		BIGINT,
	@SutureSignPatientId	INT,
	@UpdatedByUserId		INT = NULL
AS
BEGIN
	DECLARE @FirstName VARCHAR(50),
			@MiddleName VARCHAR(50),
			@LastName VARCHAR(50),
			@Suffix VARCHAR(4),
			@Last4Ssn VARCHAR(4),
			@SSN VARCHAR(9),
			@DOB DATE,
			@Gender CHAR(1),
			@Address1 VARCHAR(150),
			@Address2 VARCHAR(150),
			@City VARCHAR(150),
			@State VARCHAR(2),
			@Zip VARCHAR(9),
			@FacilityId INT,
			@FacilityMrn VARCHAR(50);

	SELECT
		@FirstName = p.FirstName,
		@MiddleName = p.MiddleName,
		@LastName = p.LastName,
		@Suffix = p.Suffix,
		@Gender = CASE p.Gender
					WHEN 1 THEN 'M'
					WHEN 2 THEN 'F'
					WHEN 3 THEN 'A'
					ELSE 'U'
				  END,
		@DOB = p.Birthdate,
		@Address1 = pa.Line1,
		@Address2 = pa.Line2,
		@City = pa.City,
		@State = pa.StateOrProvince,
		@Zip = dbo.ParseNumbers(pa.PostalCode),
		@SSN = dbo.ParseNumbers(pi_ssn.[Value]),
		@Last4Ssn = COALESCE(pi_ssn4.[Value], CASE WHEN dbo.ParseNumbers(pi_ssn.[Value]) IS NOT NULL AND LEN(dbo.ParseNumbers(pi_ssn.[Value])) = 9 THEN SUBSTRING(dbo.ParseNumbers(pi_ssn.[Value]), 6, 4) ELSE NULL END),
		@FacilityId = r.OrganizationId,
		@FacilityMrn = pi_uid.[Value],
		@UpdatedByUserId = COALESCE(@UpdatedByUserId, uf.UserId, 0)
	FROM
		dbo.RequestPatient p
			INNER JOIN dbo.RequestPatientAddress pa ON p.RequestPatientId = pa.RequestPatientId	-- WARNING: Assumes 1:1 mapping from Patient to PatientAddress
			LEFT JOIN dbo.RequestPatientIdentifier pi_ssn ON p.RequestPatientId = pi_ssn.RequestPatientId AND 'ssn' = pi_ssn.[Type]
			LEFT JOIN dbo.RequestPatientIdentifier pi_ssn4 ON p.RequestPatientId = pi_ssn4.RequestPatientId AND 'ssn4' = pi_ssn4.[Type]
			LEFT JOIN dbo.RequestPatientIdentifier pi_uid ON p.RequestPatientId = pi_uid.RequestPatientId AND 'external-unique-identifier' = pi_uid.[Type]
			INNER JOIN dbo.TransmittedRequest r ON p.RequestId = r.TransmittedRequestId
			LEFT JOIN [$(SutureSignWeb)].dbo.Users_Facilities uf ON r.SenderId = uf.Id
	WHERE
		p.RequestPatientId = @RequestPatientId;

	EXEC [$(SutureSignWeb)].dbo.spUpdatePatient
		  @PatientId = @SutureSignPatientId,
		  @FirstName = @FirstName,
		  @MiddleName = @MiddleName,
		  @LastName = @LastName,
		  @Suffix = @Suffix,
		  @Gender = @Gender,
		  @DOB = @DOB,
		  @Address1 = @Address1,
		  @Address2 = @Address2,
		  @City = @City,
		  @State = @State,
		  @Zip = @Zip,
		  @SSN = @SSN,
		  @Last4Ssn = @Last4Ssn,
		  @FacilityMrn = NULL,
		  @FacilityId = NULL,
		  @ChangeBy = @UpdatedByUserId,
		  -- Static Parameters:
		  @IsMedicare = 0,
		  @IsMedicareAdvantage = 0,
		  @IsMedicAid = 0,
		  @IsSelf = 0,
		  @IsPrivate = 0;

    DECLARE @CompanyAssociationExist BIT
    EXEC [$(SutureSignWeb)].[dbo].[spAssociatePatientToCompany]
              @CompanyMrn = @FacilityMrn,
              @FacilityId = @FacilityId,
              @PatientId  = @SutureSignPatientId,
              @ChangeBy   = @UpdatedByUserId,
              @AssociationExists = @CompanyAssociationExist OUTPUT;
              
END
