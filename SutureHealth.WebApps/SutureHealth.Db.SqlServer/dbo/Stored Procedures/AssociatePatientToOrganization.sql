CREATE PROCEDURE [dbo].[AssociatePatientToOrganization]
    @FacilityMrn varchar(50),
    @FacilityId int,
    @PatientId int,
    @ChangeBy int,
    @AssociationExists bit OUTPUT
AS

    DECLARE @FacilityAssociationExist BIT
    EXEC [$(SutureSignWeb)].[dbo].[spAssociatePatientToFacility]
              @FacilityMrn = @FacilityMrn,
              @FacilityId = @FacilityId,
              @PatientId  = @PatientId,
              @ChangeBy   = @ChangeBy,
              @AssociationExists = @FacilityAssociationExist OUTPUT;

RETURN 0
