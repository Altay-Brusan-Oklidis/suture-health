CREATE VIEW [dbo].[MemberRelationship]
AS
SELECT
	UserId			[SubordinateMemberId],
	ManagerId		[SupervisorMemberId],
	Active			[IsActive]
FROM
	[$(SutureSignWeb)].dbo.Users_Managers