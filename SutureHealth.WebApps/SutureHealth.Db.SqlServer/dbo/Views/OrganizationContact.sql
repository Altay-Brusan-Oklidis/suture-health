CREATE VIEW [dbo].[OrganizationContact]
AS 
SELECT fc.Id            OrganizationContactId,
       fc.FacilityId    OrganizationId,
       CASE
            WHEN fc.[Type] IN ('Phone', 'Fax', 'Mobile', 'Email', 'Url', 'OfficePhone', 'OfficePhoneExt') THEN fc.[Type]
            ELSE 'Phone'
       END              [Type],
       fc.[Value],
       fc.Active        IsActive,
       fc.[Primary]     IsPrimary,
       fc.CreateDate    CreatedAt,
       fc.SubmittedBy   CreatedBy,
       fc.EffectiveDate EffectiveAt,
       fc.DateMod       UpdatedAt,
       fc.UpdatedBy     UpdatedBy
  FROM [$(SutureSignWeb)].dbo.Facilities_Contacts fc WITH (NOLOCK)
