CREATE VIEW [dbo].[MemberImage]
AS 
	SELECT UserImageId 	 [MemberImageId],
		   UserId		 [MemberId],
		   IsPrimary	 [IsPrimary],
		   UploadDate    [UploadDate],
		   Active		 [Active],
		   SizeType      [SizeType]
	FROM [$(SutureSignWeb)].dbo.UserImage

