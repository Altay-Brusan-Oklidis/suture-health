CREATE VIEW [dbo].[OutcomeQueueMessage]
AS
	SELECT [DocumentQueueMessageId]		[OutcomeQueueMessageId],
		   [MessageId],
		   [RequestId]					[SutureSignRequestId],
		   [EnqueueDate],
		   [LastDequeueDate],
		   [CompletionDate],
		   [CompletionMetadata],
		   [ProcessingAttempts]
  FROM [$(SutureSignWeb)].dbo.DocumentQueueMessages