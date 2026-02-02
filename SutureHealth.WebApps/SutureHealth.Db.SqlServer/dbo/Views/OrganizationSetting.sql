CREATE VIEW [dbo].[OrganizationSetting]
AS 
	SELECT os.FacilitySettingID	[SettingId],
		   os.FacilityId		[ParentId],
		   os.Setting			[Key],
		   os.Active			[IsActive],
		   os.ItemBit			[ItemBool],
		   os.ItemInt			[ItemInt],
		   os.ItemVarChar		[ItemString],
		   os.ValueType			[ItemType]
	  FROM [$(SutureSignWeb)].dbo.FacilitySettings os
