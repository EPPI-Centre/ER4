Use Reviewer
GO
IF COL_LENGTH('dbo.TB_CONTACT', 'ARCHIE_ACCESS_TOKEN') <= 64
BEGIN 
	select 'I will do it'
	ALTER TABLE TB_CONTACT ALTER COLUMN ARCHIE_ACCESS_TOKEN VARCHAR(4000) NULL;
	ALTER TABLE TB_CONTACT ALTER COLUMN ARCHIE_REFRESH_TOKEN VARCHAR(4000) NULL;
END
select COL_LENGTH('dbo.TB_CONTACT', 'ARCHIE_ACCESS_TOKEN')
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieSaveTokens]    Script Date: 15/03/2019 15:43:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[st_ArchieSaveTokens]
	-- Add the parameters for the stored procedure here
	@ARCHIE_ID varchar(32)
	,@TOKEN varchar(4000)
	,@VALID_UNTIL datetime2(1)
	,@REFRESH_T varchar(4000)
	,@ARCHIE_CODE varchar(64) = null
	,@ARCHIE_STATE varchar(10) = null
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	
	Update TB_CONTACT set 
		ARCHIE_ACCESS_TOKEN = @TOKEN
		,ARCHIE_TOKEN_VALID_UNTIL = @VALID_UNTIL
		,ARCHIE_REFRESH_TOKEN = @REFRESH_T
		,LAST_ARCHIE_CODE = @ARCHIE_CODE
		,LAST_ARCHIE_STATE = @ARCHIE_STATE
		where ARCHIE_ID = @ARCHIE_ID
	
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ArchieSaveUnassignedTokens]    Script Date: 15/03/2019 16:19:43 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ArchieSaveUnassignedTokens]
	-- Add the parameters for the stored procedure here
	@ARCHIE_ID varchar(32)
	,@TOKEN varchar(4000)
	,@VALID_UNTIL datetime2(1)
	,@REFRESH_T varchar(4000)
	,@ARCHIE_CODE varchar(64)
	,@ARCHIE_STATE varchar(10)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	--first, make sure there are no duplicates
	delete from TB_UNASSIGNED_ARCHIE_KEYS where ARCHIE_ID = @ARCHIE_ID
	INSERT INTO [Reviewer].[dbo].[TB_UNASSIGNED_ARCHIE_KEYS]
           ([ARCHIE_ID]
           ,[ARCHIE_ACCESS_TOKEN]
           ,[ARCHIE_TOKEN_VALID_UNTIL]
           ,[ARCHIE_REFRESH_TOKEN]
           ,[LAST_ARCHIE_CODE]
           ,[LAST_ARCHIE_STATE])
     VALUES
           (@ARCHIE_ID
           ,@TOKEN
           ,@VALID_UNTIL
           ,@REFRESH_T
           ,@ARCHIE_CODE
           ,@ARCHIE_STATE)
	
END
GO


