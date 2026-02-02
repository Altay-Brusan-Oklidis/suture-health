
-- =============================================
-- Author:		kkilburn
-- Create date: 2020-11-03
-- Description:	loads final data table from raw imports
-- =============================================
CREATE PROCEDURE [dbo].[load_zipcode]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT Statements.
	SET NOCOUNT ON;

    insert into stage.zipcode
	select * 
	  from import.ZipCode
	
END
