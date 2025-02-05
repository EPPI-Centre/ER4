USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OnlineHelpPages]    Script Date: 05/02/2025 10:57:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_OnlineHelpPages]
	-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @tb_help_pages table (tv_online_help_id int, tv_context nvarchar(100))
	
	insert into @tb_help_pages (tv_online_help_id, tv_context)
	values (0, 'Select help context')

	insert into @tb_help_pages (tv_online_help_id, tv_context) 
	SELECT ONLINE_HELP_ID, CONTEXT from TB_ONLINE_HELP 
	order by ONLINE_HELP_ID

	select * from @tb_help_pages order by tv_online_help_id

END
GO

