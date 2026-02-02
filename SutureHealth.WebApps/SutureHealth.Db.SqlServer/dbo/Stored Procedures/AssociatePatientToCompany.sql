CREATE PROCEDURE [dbo].[AssociatePatientToCompany]
    @CompanyMrn varchar(50),
    @FacilityId int,
    @PatientId int,
    @ChangeBy int,
    @AssociationExists bit OUTPUT
AS

    DECLARE @CompanyAssociationExist BIT
    EXEC [$(SutureSignWeb)].[dbo].[spAssociatePatientToCompany]
              @CompanyMrn = @CompanyMrn,
              @FacilityId = @FacilityId,
              @PatientId  = @PatientId,
              @ChangeBy   = @ChangeBy,
              @AssociationExists = @CompanyAssociationExist OUTPUT;

RETURN 0
