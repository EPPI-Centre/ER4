USE [ReviewerAdmin]
GO

IF COL_LENGTH('dbo.TB_ONLINE_HELP', 'SECTION_NAME') IS NULL
BEGIN
	ALTER TABLE [dbo].[TB_ONLINE_HELP] ADD
	SECTION_NAME nvarchar(100)
END
GO

IF COL_LENGTH('dbo.TB_ONLINE_HELP', 'PARENT_CONTEXT') IS NULL
BEGIN
	ALTER TABLE [dbo].[TB_ONLINE_HELP] ADD
	PARENT_CONTEXT nvarchar(100)
END
GO


--------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[sp_OnlineHelpGet]    Script Date: 12/11/2025 13:35:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER PROCEDURE [dbo].[sp_OnlineHelpGet]
	-- Add the parameters for the stored procedure here
	@CONTEXT nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @rowCount int
	-- CONTEXT could be either CONTEXT or CONTEXT_EXTENSION
	set @rowCount = (select count(*) from TB_ONLINE_HELP where SECTION_NAME = @CONTEXT)
	if @rowCount = 1 -- it was a CONTEXT_EXTENSION
	begin 
		select * from TB_ONLINE_HELP where SECTION_NAME = @CONTEXT
	end
	else
	begin
		SELECT * from TB_ONLINE_HELP where CONTEXT = @CONTEXT
	end


END

GO

-------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OnlineHelpPages]    Script Date: 12/11/2025 13:36:25 ******/
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
	@CONTEXT nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @tb_help_pages table (tv_online_help_id int, tv_context nvarchar(100), 
		tv_section_name nvarchar(100), tv_parent_context nvarchar(100))
	
	--insert into @tb_help_pages (tv_online_help_id, tv_context)
	--values (0, 'Select a topic')

	if @CONTEXT = '0'
	begin -- this is the admin page listings so we need to know if the context is a parent
		insert into @tb_help_pages (tv_online_help_id, tv_context, tv_section_name, tv_parent_context) 
		SELECT ONLINE_HELP_ID, CONTEXT, SECTION_NAME, PARENT_CONTEXT from TB_ONLINE_HELP
		order by CONTEXT

		update @tb_help_pages
		set tv_context =  '*' + tv_context 
		where tv_parent_context is NULL
		
	end
	else
	begin
		insert into @tb_help_pages (tv_online_help_id, tv_context, tv_section_name) 
		SELECT ONLINE_HELP_ID, CONTEXT, SECTION_NAME from TB_ONLINE_HELP
		--where CONTEXT like '%' + @CONTEXT + '%'
		where PARENT_CONTEXT = @CONTEXT
		and CONTEXT != @CONTEXT
		order by CONTEXT
	end



	select tv_online_help_id, tv_context, tv_section_name from @tb_help_pages --order by tv_context

END
GO

-----------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OnlineHelpDelete]    Script Date: 12/11/2025 13:37:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER      procedure [dbo].[st_OnlineHelpDelete]
(
	@CONTEXT nchar(100) null,
	@SECTION_NAME nchar(100),
	@HELP_HTML nvarchar(max),
	@PARENT_CONTEXT nvarchar(100)
)

As

	delete from  TB_ONLINE_HELP
	where CONTEXT = @CONTEXT
	and @SECTION_NAME = @SECTION_NAME


SET NOCOUNT OFF
GO


----------------------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OnlineHelpCreateOrEdit]    Script Date: 12/11/2025 13:40:05 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER     procedure [dbo].[st_OnlineHelpCreateOrEdit]
(
	@CONTEXT nchar(100) null,
	@SECTION_NAME nchar(100),
	@HELP_HTML nvarchar(max),
	@PARENT_CONTEXT nvarchar(100)
)

As

select * from TB_ONLINE_HELP where CONTEXT = @CONTEXT
if @@ROWCOUNT = 0
begin
	-- this is a create
	-- we want to put something into the content field so it is easier to edit later
	if @HELP_HTML = ''
	begin
	set @HELP_HTML = 'Apologies, there currently is no help for this page/activity.<br />' +
				 'Please check this help page later: we are adding new Help content regularly.'
	end

	if @PARENT_CONTEXT = ''
	begin
		-- we are adding a new context
		insert into TB_ONLINE_HELP (CONTEXT, SECTION_NAME, HELP_HTML)
		values (@CONTEXT, @SECTION_NAME, @HELP_HTML)
	end
	else
	begin
		-- we are adding a context extension so we need to add the parent context 
		insert into TB_ONLINE_HELP (CONTEXT, SECTION_NAME, HELP_HTML, PARENT_CONTEXT)
		values (@CONTEXT, @SECTION_NAME, @HELP_HTML, @PARENT_CONTEXT)
	end
end
else
begin
	-- this is an edit
	update TB_ONLINE_HELP
	set HELP_HTML = @HELP_HTML,
	SECTION_NAME = @SECTION_NAME
	where CONTEXT = @CONTEXT

end


SET NOCOUNT OFF
GO

--------------------------------------------


