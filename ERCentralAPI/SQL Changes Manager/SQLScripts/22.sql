USE [DataService]
GO

/****** Object:  Table [dbo].[TB_PUBMED_UPDATE_FILE]    Script Date: 13/06/2018 11:51:22 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_PUBMED_UPDATE_FILE](
	[PUBMED_UPDATE_FILE_ID] [int] NOT NULL
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
CREATE TABLE dbo.[Tmp_TB_PUBMED_UPDATE_FILE]
	(
	PUBMED_UPDATE_FILE_ID int NOT NULL IDENTITY (1, 1),
	PUBMED_FILE_NAME varchar(30) NOT NULL,
	PUBMED_IMPORT_DATE datetime NOT NULL,
	PUBMED_UPLOAD_DATE datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.[Tmp_TB_PUBMED_UPDATE_FILE] SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.[Tmp_TB_PUBMED_UPDATE_FILE] ON
GO
IF EXISTS(SELECT * FROM dbo.[TB_PUBMED_UPDATE_FILE])
	 EXEC('INSERT INTO dbo.[Tmp_TB_PUBMED_UPDATE_FILE] (PUBMED_UPDATE_FILE_ID)
		SELECT PUBMED_UPDATE_FILE_ID FROM dbo.[TB_PUBMED_UPDATE_FILE] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.[Tmp_TB_PUBMED_UPDATE_FILE] OFF
GO
DROP TABLE dbo.[TB_PUBMED_UPDATE_FILE]
GO
EXECUTE sp_rename N'dbo.[Tmp_TB_PUBMED_UPDATE_FILE]', N'TB_PUBMED_UPDATE_FILE', 'OBJECT' 
GO
COMMIT
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
ALTER TABLE dbo.[TB_PUBMED_UPDATE_FILE] ADD CONSTRAINT
	[PK_TB_PUBMED_UPDATE_FILE] PRIMARY KEY CLUSTERED 
	(
	PUBMED_UPDATE_FILE_ID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TB_PUBMED_UPDATE_FILE_NAME] ON dbo.[TB_PUBMED_UPDATE_FILE]
	(
	PUBMED_FILE_NAME
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.[TB_PUBMED_UPDATE_FILE] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

USE [DataService]
GO

/****** Object:  StoredProcedure [dbo].[st_PubMedUpdateFileInsert]    Script Date: 13/06/2018 15:08:59 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_PubMedUpdateFileInsert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_PubMedUpdateFileInsert]
GO

/****** Object:  StoredProcedure [dbo].[st_PubMedUpdateFileInsert]    Script Date: 13/06/2018 15:08:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_PubMedUpdateFileInsert] 
	-- Add the parameters for the stored procedure here
	(
		@PUBMED_FILE_NAME varchar(30)
	,	@PUBMED_UPLOAD_DATE datetime

	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @check int
	set @check = (select PUBMED_UPDATE_FILE_ID from TB_PUBMED_UPDATE_FILE where PUBMED_FILE_NAME = @PUBMED_FILE_NAME)
	if (@check is not null and @check > 0)
	BEGIN
		--update current record: we don't want duplicate filenames in here, no matter what!
		UPDATE TB_PUBMED_UPDATE_FILE 
			set PUBMED_FILE_NAME = @PUBMED_FILE_NAME
				, PUBMED_UPLOAD_DATE = @PUBMED_UPLOAD_DATE
				, PUBMED_IMPORT_DATE = GETDATE()
			WHERE PUBMED_UPDATE_FILE_ID = @check
	END
	ELSE
	BEGIN
		--normal insert
		INSERT INTO [dbo].[TB_PUBMED_UPDATE_FILE]
           ([PUBMED_FILE_NAME]
           ,[PUBMED_IMPORT_DATE]
           ,[PUBMED_UPLOAD_DATE])
		 VALUES
           (@PUBMED_FILE_NAME
           ,GETDATE()
           ,@PUBMED_UPLOAD_DATE)

	END
END

GO


USE [DataService]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_PubMedUpdateFileGetAll]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_PubMedUpdateFileGetAll]
GO
/****** Object:  StoredProcedure [dbo].[st_PubMedUpdateFileGetAll]    Script Date: 13/06/2018 15:23:16 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_PubMedUpdateFileGetAll] 
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select * from TB_PUBMED_UPDATE_FILE
	order by PUBMED_IMPORT_DATE ASC
END
GO

