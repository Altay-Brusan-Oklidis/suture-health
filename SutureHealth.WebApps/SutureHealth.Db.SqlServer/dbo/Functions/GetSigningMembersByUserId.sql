CREATE FUNCTION [dbo].[GetSigningMembersByUserId]
(
	@memberId int
)
RETURNS @returntable TABLE
(
	ProviderId			bigint	NULL,
	Npi					bigint	NULL,
	FacilityId			int		NULL,
	UserId				int		NULL
)
AS
BEGIN

	INSERT INTO @returntable(UserId, ProviderId, Npi)
	SELECT p.SutureUserId,
		   p.ProviderId,
		   p.NPI
      FROM (SELECT um.ManagerId SutureUserId
			  FROM [$(SutureSignWeb)].dbo.users_managers um
			 WHERE um.UserId = @memberId AND um.Active = 1
		     UNION 
			SELECT @memberId) ss
	 INNER JOIN shapi.Provider p ON ss.SutureUserId = p.SutureUserId;

	RETURN
END
