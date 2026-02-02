CREATE VIEW [dbo].[Organization]
AS
SELECT f.FacilityId                             OrganizationId,
       f.ParentFacility                         ParentId,
       f.FacilityTypeId                         OrganizationTypeId,
       f.[Name],
       f.NickName                               OtherDesignation,
       f.Domain,
       f.FacilityNPI                            NPI,
       f.MedicareNumber,
       f.Active                                 IsActive,
       f.CreateDate                             CreatedAt,
       f.SubmittedBy                            CreatedBy,
       COALESCE(f.DateMod, f.CreateDate)        UpdatedAt,
       COALESCE(f.UpdatedBy, f.SubmittedBy)     UpdatedBy,
       f.EffectiveDate                          EffectiveAt,
       f.CompanyId,
       fld.Address1                             AddressLine1,
       fld.Address2                             AddressLine2,
       fld.City,
       fld.[State]                              StateOrProvince,
       fld.Zip                                  PostalCode,
       fld.Country                              CountryOrRegion,
       CASE 
		   WHEN EXISTS
		   (
				SELECT 0
	              FROM [$(SutureSignWeb)].dbo.Customers C
		         INNER JOIN [$(SutureSignWeb)].dbo.BillingAccounts BA ON C.Id = BA.CustomerId	AND BA.Active = 1 
		         INNER JOIN [$(SutureSignWeb)].dbo.BillingAccount_BillableEntities BABE ON BA.Id = BABE.BillingAccountId AND BABE.Active = 1 
		         INNER JOIN [$(SutureSignWeb)].dbo.BillableEntities BE ON BABE.BillableEntityId = BE.Id AND BE.Active = 1 
		         WHERE ((be.ObjectId = f.FacilityId AND BE.ObjectType = 'Facility') OR (be.ObjectId = f.CompanyId AND BE.ObjectType = 'Company')) AND C.Active = 1
		   ) THEN CAST(0 AS BIT)
		   ELSE CAST(1 AS BIT)
	   END										IsFree,
       f.CloseDate                              ClosedAt,
       (
            SELECT TOP 1 [Value]
            FROM [$(SutureSignWeb)].dbo.Facilities_Contacts WITH (NOLOCK)
            WHERE FacilityId = f.FacilityId AND [Type] = 'Phone' AND [Active] = 1
            ORDER BY [Primary] DESC
       )                                        PhoneNumber
  FROM [$(SutureSignWeb)].dbo.Facilities f WITH (NOLOCK)
  LEFT JOIN [$(SutureSignWeb)].dbo.Facilities_Locations fl WITH (NOLOCK) ON fl.FacilityId = f.FacilityId
  LEFT JOIN [$(SutureSignWeb)].dbo.Locations fld WITH (NOLOCK) ON fld.LocationId = fl.LocationId