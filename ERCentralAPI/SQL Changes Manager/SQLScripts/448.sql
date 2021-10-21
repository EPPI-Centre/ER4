USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Eppi_Vis_Get_Log]    Script Date: 21/10/2021 09:58:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_Eppi_Vis_Get_Log] 
	-- Add the parameters for the stored procedure here
(
	@WEBDB_ID int,
	@FROM datetime,
	@UNTIL datetime,
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
 
	declare @tb_eppi_vis_log table (tv_webdb_log_identity int, tv_created datetime,
		tv_log_type nvarchar(25), tv_details nvarchar(max))

	if @UNTIL = '1980/01/01 00:00:00'
	begin
		insert into @tb_eppi_vis_log (tv_webdb_log_identity, tv_created, tv_log_type, tv_details)
		select WEBDB_LOG_IDENTITY, CREATED, LOG_TYPE, DETAILS from TB_WEBDB_LOG
		where WEBDB_ID = @WEBDB_ID
		and CREATED >= @FROM
	end
	else
	begin
		insert into @tb_eppi_vis_log (tv_webdb_log_identity, tv_created, tv_log_type, tv_details)
		select WEBDB_LOG_IDENTITY, CREATED, LOG_TYPE, DETAILS from TB_WEBDB_LOG
		where WEBDB_ID = @WEBDB_ID
		and CREATED >= @FROM
		and CREATED <= @UNTIL
	end


	select * from @tb_eppi_vis_log
	where ((tv_log_type like '%' + @TEXT_BOX + '%') OR
			(tv_details like '%' + @TEXT_BOX + '%'))


       
END



GO


-----------------------------------------------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_EPPI_Vis_Name_and_Review]    Script Date: 21/10/2021 09:59:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_EPPI_Vis_Name_and_Review]
(
	@WEBDB_ID nvarchar(10),
	@WEBDB_NAME nvarchar(200) output,
	@REVIEW_NAME nvarchar(200) output,
	@REVIEW_ID nvarchar(10) output
)

As

SET NOCOUNT ON

	set @WEBDB_NAME = (select WEBDB_NAME from Reviewer.dbo.TB_WEBDB where WEBDB_ID = @WEBDB_ID)
	set @REVIEW_ID = (select REVIEW_ID from Reviewer.dbo.TB_WEBDB where WEBDB_ID = @WEBDB_ID)
	set @REVIEW_NAME = (select REVIEW_NAME from Reviewer.dbo.TB_REVIEW where REVIEW_ID = @REVIEW_ID)



SET NOCOUNT OFF


GO

