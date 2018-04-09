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