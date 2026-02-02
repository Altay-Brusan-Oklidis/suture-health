USE [SutureSignApi-QA]
select * from [dbo].[Patient]
where FirstName = 'aunidonumo'
order by [CreatedAt] desc

USE [SutureSignWeb-QA]
select * from [dbo].[Patients]
where FirstName = 'aunidonumo'
order by [CreateDate] desc