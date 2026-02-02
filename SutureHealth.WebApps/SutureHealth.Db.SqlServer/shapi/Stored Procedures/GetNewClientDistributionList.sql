CREATE PROCEDURE [shapi].[GetNewClientDistributionList]
   @lookback int = -24,
   @distance int = 100,
   @jsonOutput NVARCHAR(MAX) OUTPUT
AS
BEGIN

  WITH c(lat,long,email)
  AS
  (
    SELECT e.Latitude, 
	       e.Longitude, 
	       email = e.Value
      FROM (SELECT DISTINCT p.Latitude, p.Longitude, uc.Value
		      FROM shapi.Provider n
		     INNER JOIN shapi.Provider p ON NOT p.SutureFacilityId IS NULL AND n.ProviderId <> p.ProviderId AND (geography::Point(COALESCE(p.latitude, 0), COALESCE(p.longitude, 0), 4326).STDistance(geography::Point(COALESCE(n.latitude, 0), COALESCE(n.longitude, 0), 4326))) < (@distance * 1609.344)
		     INNER JOIN [$(SutureSignWeb)].dbo.Users_Facilities fu ON p.suturefacilityid = fu.facilityID AND fu.Active = 1
		     INNER JOIN [$(SutureSignWeb)].dbo.Users u ON fu.UserId = u.UserId AND u.Active = 1 AND u.UserTypeId <> 2000
		     INNER JOIN [$(SutureSignWeb)].dbo.Users_Contacts uc ON u.UserId = uc.UserId AND uc.Active = 1 AND uc.Type = 'Email'
             WHERE NOT n.SutureCreatedAt is null 
		           AND n.SutureCreatedAt > DATEADD(hh, @lookback, GETUTCDATE())
		           AND p.SutureCustomerType > 0) e
  )
  SELECT @jsonOutput = (SELECT a.lat, a.long, email = (SELECT JSON_QUERY(concat('[' , STRING_AGG(concat('"' , STRING_ESCAPE(Email, 'json') , '"'),',') , ']')) 
													     FROM c
													    WHERE a.lat = c.lat AND a.long = c.long)
						  FROM (SELECT DISTINCT c.lat, c.long FROM c) a
						   FOR JSON PATH);
END;
