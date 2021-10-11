USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Eppi_Vis_Get_All]    Script Date: 28/04/2021 10:39:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_Eppi_Vis_Get_All] 
	-- Add the parameters for the stored procedure here
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	declare @tb_eppi_vis table (tv_webdb_id int, tv_webdb_name nvarchar(1000),
		tv_review_id int, tv_review_name nvarchar(1000), tv_contact_id int,
		tv_contact_name nvarchar(255), tv_is_open bit)

	insert into @tb_eppi_vis (tv_webdb_id, tv_webdb_name, tv_review_id, tv_contact_id, tv_is_open)
	select WEBDB_ID, WEBDB_NAME, REVIEW_ID, CREATED_BY, IS_OPEN from Reviewer.dbo.TB_WEBDB

	update @tb_eppi_vis
	set tv_contact_name = CONTACT_NAME
	from Reviewer.dbo.TB_CONTACT
	where tv_contact_id = CONTACT_ID 

	update @tb_eppi_vis
	set tv_review_name = REVIEW_NAME
	from Reviewer.dbo.TB_REVIEW
	where tv_review_id = REVIEW_ID 



	select * from @tb_eppi_vis
	where ((tv_webdb_id like '%' + @TEXT_BOX + '%') OR
					(tv_webdb_name like '%' + @TEXT_BOX + '%') OR
					(tv_review_name like '%' + @TEXT_BOX + '%') OR
					(tv_contact_name like '%' + @TEXT_BOX + '%')OR
					(tv_review_id like '%' + @TEXT_BOX + '%'))


       
END





GO