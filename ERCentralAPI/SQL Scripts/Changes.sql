USE [ReviewerAdmin]
GO

/****** Object:  Table [dbo].[TB_CHECKLINK]    Script Date: 02/21/2018 12:06:56 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_PERSISTED_GRANT]') AND type in (N'U'))
DROP TABLE [dbo].[TB_PERSISTED_GRANT]
GO

/****** Object:  Table [dbo].[TB_CHECKLINK]    Script Date: 02/21/2018 12:06:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[TB_PERSISTED_GRANT](
	[PERSISTED_GRANT_ID] [int] IDENTITY(1,1) NOT NULL,
	[KEY] [nvarchar](200) NOT NULL,
	[TYPE] [nvarchar](50) NOT NULL,
	[CLIENT_ID] [nvarchar](200) NOT NULL,
	[DATE_CREATED] [datetime2](1) NOT NULL,
	[DATA] [nvarchar](MAX) NOT NULL,
	[EXPIRATION] [datetime2](1) NOT NULL,
	[CONTACT_ID] [int] NULL,
	
 CONSTRAINT [PK_TB_PERSISTED_GRANT] PRIMARY KEY CLUSTERED 
(
	[PERSISTED_GRANT_ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE st_PersistedGrantAdd
	-- Add the parameters for the stored procedure here
	@KEY nvarchar(200),
	@TYPE nvarchar(50),
	@CLIENT_ID [nvarchar](200),
	@DATE_CREATED datetime2(1),
	@DATA nvarchar(MAX),
	@EXPIRATION datetime2(1),
	@CONTACT_ID [int]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	    declare @check int
    select @check = PERSISTED_GRANT_ID from TB_PERSISTED_GRANT where [KEY] = @KEY
    if @check is null
    BEGIN
		INSERT INTO [TB_PERSISTED_GRANT]
			   ([KEY]
			   ,[TYPE]
			   ,[CLIENT_ID]
			   ,[DATE_CREATED]
			   ,[DATA]
			   ,[EXPIRATION]
			   ,[CONTACT_ID])
		 VALUES
			   (@KEY ,
				@TYPE,
				@CLIENT_ID,
				@DATE_CREATED,
				@DATA,
				@EXPIRATION,
				@CONTACT_ID)
	END
	ELSE
	BEGIN
		UPDATE TB_PERSISTED_GRANT
			SET [KEY] = @KEY
			   ,[TYPE] = @TYPE
			   ,[CLIENT_ID] = @CLIENT_ID
			   ,[DATE_CREATED] = @DATE_CREATED
			   ,[DATA] = @DATA
			   ,[EXPIRATION] = @EXPIRATION
			   ,[CONTACT_ID] = @CONTACT_ID
			WHERE [KEY] = @KEY
	END
END
GO

CREATE PROCEDURE st_PersistedGrantGet
	-- Add the parameters for the stored procedure here
	@KEY nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select * from  TB_PERSISTED_GRANT
	Where [KEY] = @KEY
END
GO

CREATE PROCEDURE st_PersistedGrantGetAll
	-- Add the parameters for the stored procedure here
	@CONTACT_ID [int]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select * from  TB_PERSISTED_GRANT
	Where CONTACT_ID = @CONTACT_ID
END
GO

CREATE PROCEDURE st_PersistedGrantRemoveAll
	-- Add the parameters for the stored procedure here
	@CONTACT_ID [int],
	@CLIENT_ID [nvarchar](200),
	@TYPE nvarchar(50)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	IF @TYPE = 'ALLTYPES'
	BEGIN
		DELETE from TB_PERSISTED_GRANT where [CONTACT_ID] = @CONTACT_ID AND [CLIENT_ID] = @CLIENT_ID
	END
	ELSE
	BEGIN
		DELETE from TB_PERSISTED_GRANT where [CONTACT_ID] = @CONTACT_ID AND [CLIENT_ID] = @CLIENT_ID AND [TYPE] = @TYPE 
	END
	
END
GO

CREATE PROCEDURE st_PersistedGrantRemove
	-- Add the parameters for the stored procedure here
	@KEY nvarchar(200)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	DELETE from TB_PERSISTED_GRANT where [KEY] = @KEY
END
GO

/*--------------------------------------------------------------------------------------*/
/*--------------------------------------------------------------------------------------*/
/*--------------------------------------------------------------------------------------*/
--ADDED 09/Apr/2018
--The bit below adds a table to keep track of the current DB version, this is used for Continuous Integration, to apply the required SQL changes scripts.
--This script is outside the scope of such automation, as it is required for the process to work.
/*--------------------------------------------------------------------------------------*/
/*--------------------------------------------------------------------------------------*/
/*--------------------------------------------------------------------------------------*/

USE [ReviewerAdmin]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_DB_VERSION_DATE_APPLIED]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[TB_DB_VERSION] DROP CONSTRAINT [DF_TB_DB_VERSION_DATE_APPLIED]
END

GO

USE [ReviewerAdmin]
GO

/****** Object:  Table [dbo].[TB_DB_VERSION]    Script Date: 04/09/2018 14:44:53 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_DB_VERSION]') AND type in (N'U'))
DROP TABLE [dbo].[TB_DB_VERSION]
GO

USE [ReviewerAdmin]
GO

/****** Object:  Table [dbo].[TB_DB_VERSION]    Script Date: 04/09/2018 14:44:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TB_DB_VERSION](
	[VERSION_ID] [int] NOT NULL,
	[VERSION_NUMBER] [int] NOT NULL,
	[DATE_APPLIED] [datetime] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[TB_DB_VERSION] ADD  CONSTRAINT [DF_TB_DB_VERSION_DATE_APPLIED]  DEFAULT (getdate()) FOR [DATE_APPLIED]
GO

USE [ReviewerAdmin]
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
ALTER TABLE dbo.TB_DB_VERSION
	DROP CONSTRAINT DF_TB_DB_VERSION_DATE_APPLIED
GO
CREATE TABLE dbo.Tmp_TB_DB_VERSION
	(
	VERSION_ID int NOT NULL IDENTITY (1, 1),
	VERSION_NUMBER int NOT NULL,
	DATE_APPLIED datetime NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_TB_DB_VERSION SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_TB_DB_VERSION ADD CONSTRAINT
	DF_TB_DB_VERSION_DATE_APPLIED DEFAULT (getdate()) FOR DATE_APPLIED
GO
SET IDENTITY_INSERT dbo.Tmp_TB_DB_VERSION ON
GO
IF EXISTS(SELECT * FROM dbo.TB_DB_VERSION)
	 EXEC('INSERT INTO dbo.Tmp_TB_DB_VERSION (VERSION_ID, VERSION_NUMBER, DATE_APPLIED)
		SELECT VERSION_ID, VERSION_NUMBER, DATE_APPLIED FROM dbo.TB_DB_VERSION WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_TB_DB_VERSION OFF
GO
DROP TABLE dbo.TB_DB_VERSION
GO
EXECUTE sp_rename N'dbo.Tmp_TB_DB_VERSION', N'TB_DB_VERSION', 'OBJECT' 
GO
COMMIT
GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_DbVersionAdd]    Script Date: 04/09/2018 14:58:36 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_DbVersionAdd]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_DbVersionAdd]
GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_DbVersionAdd]    Script Date: 04/09/2018 14:58:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_DbVersionAdd]
(
	@VERSION_NUMBER int 
)	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	INSERT INTO [TB_DB_VERSION]
           ([VERSION_NUMBER], [DATE_APPLIED])
     VALUES
           (@VERSION_NUMBER, getdate())

END

GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_DbVersionGet]    Script Date: 04/09/2018 15:00:33 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_DbVersionGet]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_DbVersionGet]
GO

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_DbVersionGet]    Script Date: 04/09/2018 15:00:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_DbVersionGet] 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	Select top(1) * from TB_DB_VERSION order by VERSION_ID desc
END

GO

