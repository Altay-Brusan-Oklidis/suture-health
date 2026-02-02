CREATE VIEW [dbo].[SystemBillableEntity]
		AS SELECT Id					[BillableEntityId],
			      ObjectId				[OrganizationId],
				  be.ObjectType			[OrganizationType],
				  bes.SysServiceId		[SystemServiceId],
				  bes.Active			[IsSubscribed]				  
				  FROM [$(SutureSignWeb)].dbo.[BillableEntity_SysServices] as bes
							JOIN [$(SutureSignWeb)].dbo.[BillableEntities] as be
							ON be.Id = bes.BillableEntityId Where be.Active = 1						
