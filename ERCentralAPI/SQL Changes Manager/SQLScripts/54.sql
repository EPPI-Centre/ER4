USE [ReviewerAdmin]
GO
declare @Content nvarchar(max) = '<p class="font-weight-bold">Item Details (Coding UI) Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('(codingui)itemdetails'
           ,@Content)
GO

declare @Content nvarchar(max) = '<p class="font-weight-bold">Home page (Coding UI) Help</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('(codingui)main'
           ,@Content)
GO

declare @Content nvarchar(max) = '<p class="font-weight-bold">Item Details Page</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('itemdetails'
           ,@Content)
GO

declare @Content nvarchar(max) = '<p class="font-weight-bold">Item Details Page, Study Arms Tab</p>
<p>...Work in progress (our apologies)... Please check this help page later: we are adding new Help content regularly.
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('itemdetails\arms'
           ,@Content)
GO


