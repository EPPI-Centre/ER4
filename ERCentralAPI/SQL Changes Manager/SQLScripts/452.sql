USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_WebDBWriteToLog]    Script Date: 03/11/2021 11:20:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER     procedure [dbo].[st_WebDBWriteToLog] (
 
	@WebDbId int
	, @ReviewID int
	, @Type nvarchar(25)
	, @Details nvarchar(max)
)

As

SET NOCOUNT ON


	if @Type = 'GetSetFrequency'
	begin
		set @Details = (select SET_NAME from Reviewer.dbo.TB_SET where SET_ID = @Details)
	end

	if @Type = 'GetFrequency'
	begin
		set @Details = (select ATTRIBUTE_NAME from Reviewer.dbo.TB_ATTRIBUTE where ATTRIBUTE_ID = @Details)
	end

	if @Type = 'ItemDetailsFromList'
	begin
		declare @itemID nvarchar(20) = @Details
		set @Details = @itemID + ',' + 
		(select SHORT_TITLE from Reviewer.dbo.TB_ITEM where ITEM_ID = @itemID)
	end
	
	insert into TB_WEBDB_LOG (WEBDB_ID, REVIEW_ID, LOG_TYPE, DETAILS)
	values (@WebDbId, @ReviewID, @Type, @Details)


	
SET NOCOUNT OFF
GO

--------------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_Eppi_Vis_Get_Log]    Script Date: 03/11/2021 11:21:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_Eppi_Vis_Get_Log] 
	-- Add the parameters for the stored procedure here
(
	@WEBDB_ID int,
	@FROM datetime,
	@UNTIL datetime,
	@TYPE nvarchar(255)
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
		select top 5000 WEBDB_LOG_IDENTITY, CREATED, LOG_TYPE, DETAILS from TB_WEBDB_LOG
		where WEBDB_ID = @WEBDB_ID
		and CREATED >= @FROM
	end
	else
	begin
		insert into @tb_eppi_vis_log (tv_webdb_log_identity, tv_created, tv_log_type, tv_details)
		select top 5000 WEBDB_LOG_IDENTITY, CREATED, LOG_TYPE, DETAILS from TB_WEBDB_LOG
		where WEBDB_ID = @WEBDB_ID
		and CREATED >= @FROM
		and CREATED <= @UNTIL
	end

	if @TYPE = 'All'
	begin
		select * from @tb_eppi_vis_log 
		order by tv_created desc
	end
	else
	begin
		select * from @tb_eppi_vis_log 
		where tv_log_type like @TYPE
		order by tv_created desc
	end

	--select * from @tb_eppi_vis_log
	--where ((tv_log_type like '%' + @TEXT_BOX + '%') OR
	--		(tv_details like '%' + @TEXT_BOX + '%'))


       
END



GO

