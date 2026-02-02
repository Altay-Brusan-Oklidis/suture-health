CREATE VIEW [dbo].[IntegratorSenderDocumentKeys]
AS 
SELECT io.ApiKey	'SenderId',
	   i.ApiKey		'IntegratorId',
	   tc.DocumentTypeKey
  from dbo.IntegratorOrganization [io]
 inner join [$(SutureSignWeb)].dbo.Templates t ON [io].OrganizationId = t.FacilityId
 inner join dbo.TemplateConfiguration tc ON t.TemplateId = tc.TemplateId
 inner join dbo.Integrator i ON [io].IntegratorId = i.IntegratorId
