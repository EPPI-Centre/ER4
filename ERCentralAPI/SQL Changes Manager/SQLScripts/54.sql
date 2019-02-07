USE [ReviewerAdmin]
GO
declare @ctx nvarchar(100) = '(codingui)itemdetails'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Item Details (Coding UI) Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = '(codingui)main'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Home page (Coding UI) Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = 'itemdetails'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Item Details Page</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = 'itemdetails\arms'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Item Details Page, Study Arms Tab</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = 'sources\file'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Sources Page, Import file tab</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = 'sources\pubmed'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Sources Page, PubMed Tab</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = 'sources\managesources'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Sources Page, Manage Sources Tab</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO


declare @ctx nvarchar(100) = 'editcodesets'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Edit CodeSets Page</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO

declare @ctx nvarchar(100) = 'importcodesets'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Import CodeSets Page</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO


declare @ctx nvarchar(100) = 'buildmodel'
declare @Content nvarchar(max) = '<p class="font-weight-bold">Build Model Page</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
IF exists (select * from TB_ONLINE_HELP where CONTEXT = @ctx)
BEGIN
	update TB_ONLINE_HELP set HELP_HTML = @Content where CONTEXT = @ctx
END
ELSE
BEGIN
	INSERT into TB_ONLINE_HELP ([CONTEXT]
			   ,[HELP_HTML])
		 VALUES
			   (@ctx
			   ,@Content)
END
GO



