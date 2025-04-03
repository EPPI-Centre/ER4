USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OnlineHelpCreateOrEdit]    Script Date: 10/01/2025 12:02:26 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   procedure [dbo].[st_OnlineHelpCreateOrEdit]
(
	@CONTEXT nchar(100) null,
	@HELP_HTML nvarchar(max)
)

As

select * from TB_ONLINE_HELP where CONTEXT = @CONTEXT
if @@ROWCOUNT = 0
begin
	-- this is a create
	insert into TB_ONLINE_HELP (CONTEXT, HELP_HTML)
	values (@CONTEXT, @HELP_HTML)
end
else
begin
	-- this is an edit
	update TB_ONLINE_HELP
	set HELP_HTML = @HELP_HTML
	where CONTEXT = @CONTEXT

end


SET NOCOUNT OFF
GO

