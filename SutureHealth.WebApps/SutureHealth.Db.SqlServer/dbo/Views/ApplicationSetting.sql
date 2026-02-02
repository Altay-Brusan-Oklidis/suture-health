CREATE VIEW [dbo].[ApplicationSetting]
AS 
	SELECT ss.SystemId			[SettingId],
		   NULL					[ParentId],
		   ss.Setting			[Key],
		   ss.Active			[IsActive],
		   ss.ItemBit			[ItemBool],
		   ss.ItemInt			[ItemInt],
		   ss.ItemVarChar		[ItemString],
		   ss.ValueType			[ItemType]
	  FROM [$(SutureSignWeb)].dbo.SystemSettings ss
