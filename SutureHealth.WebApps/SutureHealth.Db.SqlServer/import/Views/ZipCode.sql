CREATE VIEW [import].[ZipCode]
AS 
	select FORMAT(ZIPCode, '00000')	AS PostalCodeArea,
		   null						AS BeginingRoute,
		   null						AS EndingRoute,
		   Latitude,
		   Longitude
	  from import.[uszc-geo-full]
	 union all
	select FORMAT(ZIPCode, '00000'),
		   beg4,
		   end4,
		   latitude,
		   Longitude
	  from import.[zip4ll-full]

