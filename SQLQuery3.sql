--USE [SutureSignApi-QA]
--select top 10 * from [dbo].[PatientMatchLog]
--where FirstName = 'aunidonumo'
--order by [CreateDate] desc

--USE [SutureSignWeb-QA]
--select top 10 * from [dbo].[MatchPatientLog]
--where [SubmittedFirstName] = 'aunidonumo'
--order by [CreateDate] desc

USE [SutureSignApi-QA]
select top 10 * from [dbo].[PatientMatchLog]
where FirstName = 'aunidonumo'
order by [CreateDate] desc

USE [SutureSignWeb-QA]
select top 10 * from [dbo].[MatchPatientLog]
where FirstName = 'aunidonumo'
order by [CreateDate] desc