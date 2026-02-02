CREATE VIEW [dbo].[PatientIdentifier]
AS
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, PolicyNumber AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'mbi' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 1 AND PlanId = 4 AND Active = 1 AND LEN(PolicyNumber) > 0	-- mbi
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, PolicyNumber AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'medicaid-number' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 3 AND PlanId = 3 AND Active = 1 AND LEN(PolicyNumber) > 0		-- medicaid-number
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, [State] AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'medicaid-state' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 3 AND PlanId = 3 AND PolicyNumber IS NOT NULL AND [State] IS NOT NULL AND Active = 1	-- medicaid-state
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, 'True' AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'has-medicare' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 1 AND PlanId = 1 AND Active = 1	-- has-medicare
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, 'True' AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'has-medicare-advantage' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 2 AND PlanId = 2 AND Active = 1	-- has-medicare-advantage
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, 'True' AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'has-self-pay' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 4 AND Active = 1	-- has-self-pay
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, 'True' AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'has-private-insurance' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 5 AND Active = 1	-- has-private-insurance
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, 'True' AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'has-medicaid' AS [Type]
FROM [$(SutureSignWeb)].dbo.PatientIns WITH (NOLOCK) WHERE
	CarrierId = 3 AND PlanId = 3 AND PolicyNumber IS NOT NULL AND [State] IS NOT NULL AND Active = 1
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, CAST(LastSSN AS VARCHAR(MAX)) AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'ssn4' AS [Type]
FROM [$(SutureSignWeb)].dbo.Patients WITH (NOLOCK) WHERE LastSSN IS NOT NULL AND LEN(LastSSN) > 0 AND Active = 1
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, CAST(SSN AS VARCHAR(MAX)) AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'ssn' AS [Type]
FROM [$(SutureSignWeb)].dbo.Patients WITH (NOLOCK) WHERE SSN IS NOT NULL AND LEN(SSN) > 0 AND Active = 1
UNION ALL
SELECT
	PatientId, CAST(CHECKSUM(NEWID()) AS BIGINT) AS PatientIdentifierId, CAST(PatientId AS VARCHAR(MAX)) AS [Value], CreateDate AS CreatedAt, ChangeDate AS UpdatedAt, 'suture-unique-identifier' AS [Type]
FROM [$(SutureSignWeb)].dbo.Patients WITH (NOLOCK) WHERE Active = 1