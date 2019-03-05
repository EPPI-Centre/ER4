USE [ReviewerAdmin]
GO
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ONLINE_HELP')
BEGIN
/****** Object:  Table [dbo].[TB_ONLINE_HELP]    Script Date: 05/02/2019 10:14:58 ******/
DROP TABLE [dbo].[TB_ONLINE_HELP]
END
GO

/****** Object:  Table [dbo].[TB_ONLINE_HELP]    Script Date: 05/02/2019 10:14:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_ONLINE_HELP](
	[ONLINE_HELP_ID] [int] NOT NULL
) ON [PRIMARY]
GO


/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_TB_ONLINE_HELP
	(
	ONLINE_HELP_ID int NOT NULL IDENTITY (1, 1),
	CONTEXT nvarchar(100) NOT NULL,
	HELP_HTML nvarchar(MAX) NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_TB_ONLINE_HELP SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_TB_ONLINE_HELP ON
GO
IF EXISTS(SELECT * FROM dbo.TB_ONLINE_HELP)
	 EXEC('INSERT INTO dbo.Tmp_TB_ONLINE_HELP (ONLINE_HELP_ID)
		SELECT ONLINE_HELP_ID FROM dbo.TB_ONLINE_HELP WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_TB_ONLINE_HELP OFF
GO
DROP TABLE dbo.TB_ONLINE_HELP
GO
EXECUTE sp_rename N'dbo.Tmp_TB_ONLINE_HELP', N'TB_ONLINE_HELP', 'OBJECT' 
GO
COMMIT
GO

USE [ReviewerAdmin]

GO

SET ANSI_PADDING ON


GO

CREATE UNIQUE CLUSTERED INDEX [IX_TB_ONLINE_HELP_CONTEXT] ON [dbo].[TB_ONLINE_HELP]
(
	[CONTEXT] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)

GO



USE [ReviewerAdmin] 
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_OnlineHelpGet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_OnlineHelpGet]
GO

CREATE PROCEDURE sp_OnlineHelpGet
	-- Add the parameters for the stored procedure here
	@CONTEXT nvarchar(100)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT * from TB_ONLINE_HELP where CONTEXT = @CONTEXT
END
GO

declare @Content nvarchar(max) = '
<p class="font-weight-bold">Welcome-Page Help</p>
<p>This page allows to choose the review you want to work on, and/or create a new (private) review.</p>
<p>To create a new (private) review, please click on the "Create Review", type a name for the review and click "Create".<br />
<span class="small">The new review will be ''private'', to share the review, you will need to purchase a subscription via the 
<a href="https://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935" target="_blank">online shop</a>.</span></p>
<p>You can open your reviews in 2 ways:
<ol>
<li>To open the Full user inteface, click on the review name.</li>
<li>To open the Coding Interface (optimised for small screens), click on the "Coding UI" button.</li>
</ol>
</p>
'
INSERT into TB_ONLINE_HELP ([CONTEXT]
           ,[HELP_HTML])
     VALUES
           ('intropage'
           ,@Content)
GO