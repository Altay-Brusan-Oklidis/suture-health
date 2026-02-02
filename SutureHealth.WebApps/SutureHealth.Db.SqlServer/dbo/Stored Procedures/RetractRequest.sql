CREATE PROCEDURE [dbo].[RetractRequest]
	@RequestId BIGINT,
	@SubmitterUserFacilityId INT = NULL
AS
BEGIN
	DECLARE @LegacyRequestId BIGINT = NULL,
			@PatientId INT = NULL,
			@TemplateId INT = NULL,
			@SubmitterUserId INT = NULL,
			@SubmitterFacilityId INT = NULL;

	SELECT
		@LegacyRequestId = nr.SutureSignRequestID,
		@PatientId = lr.Patient,
		@TemplateId = lr.Template,
		@SubmitterUserFacilityId = COALESCE(@SubmitterUserFacilityId, lr.Submitter)
	FROM
		dbo.TransmittedRequest nr
			INNER JOIN [$(SutureSignWeb)].dbo.Requests lr ON nr.SutureSignRequestID = lr.Id
	WHERE
		TransmittedRequestId = @RequestId;

	-- Don't do anything if a request hasn't made it into the legacy SutureSign system yet.
	IF (@LegacyRequestId IS NULL)
		RETURN;

	SELECT
		@SubmitterUserId = UserId,
		@SubmitterFacilityId = FacilityId
	FROM
		[$(SutureSignWeb)].dbo.Users_Facilities
	WHERE
		Id = @SubmitterUserFacilityId;

	-- Only insert a task if one of the same ActionId hasn't already been inserted.
	INSERT INTO [$(SutureSignWeb)].dbo.Tasks
	(
		[FormId],
		[UserId],
		[FacilityId],
		[ActionId],
		[TemplateId],
		[PatientId],
		[SubmittedBy],
		[Data],
		[CreateDate],
		[Active],
		[SubmittedByFacility]
	)
	SELECT
		@LegacyRequestId,
		@SubmitterUserId,
		@SubmitterFacilityId,
		new_tasks.ActionId,
		@TemplateId,
		@PatientId,
		@SubmitterUserId,
		new_tasks.DataText,
		dbo.GetSutureSignDate(),
		1,
		@SubmitterFacilityId
	FROM
		(
			SELECT
				549		ActionId,
				'Incomplete (' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy') + ')'	DataText
		) new_tasks
			LEFT JOIN [$(SutureSignWeb)].dbo.Tasks existing_retract_task ON @LegacyRequestId = existing_retract_task.FormId AND 549 = existing_retract_task.ActionId AND 1 = existing_retract_task.Active
	WHERE
		existing_retract_task.TaskId IS NULL;

	-- Only insert a task if one of the same ActionId hasn't already been inserted.
	INSERT INTO [$(SutureSignWeb)].dbo.Tasks
	(
		[FormId],
		[UserId],
		[FacilityId],
		[ActionId],
		[TemplateId],
		[PatientId],
		[SubmittedBy],
		[Data],
		[CreateDate],
		[Active],
		[SubmittedByFacility]
	)
	SELECT
		@LegacyRequestId,
		@SubmitterUserId,
		@SubmitterFacilityId,
		new_tasks.ActionId,
		@TemplateId,
		@PatientId,
		@SubmitterUserId,
		new_tasks.DataText,
		dbo.GetSutureSignDate(),
		1,
		@SubmitterFacilityId
	FROM
		(
			SELECT
				547		ActionId,
				'Archived (' + FORMAT(dbo.GetSutureSignDate(), 'M/d/yyyy') + ')'		DataText
		) new_tasks
			LEFT JOIN [$(SutureSignWeb)].dbo.Tasks existing_archive_task ON @LegacyRequestId = existing_archive_task.FormId AND 547 = existing_archive_task.ActionId AND 1 = existing_archive_task.Active
	WHERE
		existing_archive_task.TaskId IS NULL;
END