CREATE PROCEDURE [dbo].[AddCpoEntry]
	@MemberId INT,
	@OrganizationId INT,
	@PatientId INT,
	@SutureSignRequestId INT,
	@Type INT,
	@Minutes INT,
	@EffectiveAt DATE,
	@Comment VARCHAR(MAX),
	@CpoTypeIds IntegerKey READONLY
AS
BEGIN
	INSERT INTO [$(SutureSignWeb)].dbo.TimeTracking
	(
		UserId,
		FacilityId,
		PatientId,
		[Type],
		[Date],
		[Minutes],
		Comment,
		RequestId,
		CreateDate
	)
	VALUES
	(
		@MemberId,
		@OrganizationId,
		@PatientId,
		@Type,
		@EffectiveAt,
		@Minutes,
		@Comment,
		@SutureSignRequestId,
		GETDATE()
	);

	INSERT INTO [$(SutureSignWeb)].dbo.TimeTracking_Selections
	(
		TimeTrackingId,
		SelectionId
	)
	SELECT
		SCOPE_IDENTITY(),
		Id
	FROM @CpoTypeIds;
END