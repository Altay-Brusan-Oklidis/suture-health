--USE [SutureSignApi-QA]
--select top 200 * from [dbo].[PatientMatchOutcome]
--where [PatientID]=1012333
--order by [CreateDate] desc
--go

--use [SutureSignWeb-QA]
--select top 200 * from [dbo].[MatchPatientOutcome]
--where [PatientID]=1012333
--order by [CreateDate] desc

USE [SutureSignApi-QA]
select top 200 * from [dbo].[PatientMatchOutcome]
where MatchPatientLogID=16323
order by [CreateDate] desc
go

use [SutureSignWeb-QA]
select top 200 * from [dbo].[MatchPatientOutcome]
where MatchPatientLogID=16323
order by [CreateDate] desc