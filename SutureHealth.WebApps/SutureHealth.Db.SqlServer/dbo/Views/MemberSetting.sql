CREATE VIEW [dbo].[MemberSetting]
AS 
	SELECT us.UserSettingId [SettingId],
		   us.UserId		[ParentId],
		   us.Setting		[Key],
		   us.Active		[IsActive],
		   us.ItemBit		[ItemBool],
		   us.ItemInt		[ItemInt],
		   us.ItemVarChar	[ItemString],
		   us.ValueType		[ItemType]
	  FROM [$(SutureSignWeb)].dbo.UserSettings us
