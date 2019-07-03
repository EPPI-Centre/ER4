Use Reviewer
GO
IF COL_LENGTH('dbo.TB_CONTACT', 'ARCHIE_ACCESS_TOKEN') <= 64
BEGIN 
	--select 'I will do it'
	ALTER TABLE TB_CONTACT ALTER COLUMN ARCHIE_ACCESS_TOKEN VARCHAR(4000) NULL;
	ALTER TABLE TB_CONTACT ALTER COLUMN ARCHIE_REFRESH_TOKEN VARCHAR(4000) NULL;
	ALTER TABLE TB_CONTACT ALTER COLUMN LAST_ARCHIE_CODE VARCHAR(4000) NULL;
	ALTER TABLE TB_CONTACT ALTER COLUMN LAST_ARCHIE_STATE VARCHAR(12) NULL;
	ALTER TABLE TB_UNASSIGNED_ARCHIE_KEYS ALTER COLUMN LAST_ARCHIE_CODE VARCHAR(4000) NULL;
	ALTER TABLE TB_UNASSIGNED_ARCHIE_KEYS ALTER COLUMN LAST_ARCHIE_STATE VARCHAR(12) NULL;
	ALTER TABLE TB_UNASSIGNED_ARCHIE_KEYS ALTER COLUMN ARCHIE_ACCESS_TOKEN VARCHAR(4000) NULL;
	ALTER TABLE TB_UNASSIGNED_ARCHIE_KEYS ALTER COLUMN ARCHIE_REFRESH_TOKEN VARCHAR(4000) NULL;
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
	,@ARCHIE_CODE varchar(4000) = null
	,@ARCHIE_STATE varchar(12) = null
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
	,@ARCHIE_CODE varchar(4000)
	,@ARCHIE_STATE varchar(12)
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
ALTER PROCEDURE [dbo].[st_ArchieIdentityFromUnassignedCodeAndStatus]
	-- Add the parameters for the stored procedure here
	@CID int
	,@ARCHIE_CODE varchar(4000)
	,@ARCHIE_STATE varchar(12)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @ck int = (
						Select COUNT(u.ARCHIE_ID) from TB_UNASSIGNED_ARCHIE_KEYS u
						inner join TB_CONTACT c	
							on c.CONTACT_ID = @CID and c.ARCHIE_ID is null
						where @ARCHIE_CODE = u.LAST_ARCHIE_CODE and @ARCHIE_STATE = u.LAST_ARCHIE_STATE 
						and (u.LAST_ARCHIE_CODE is not null and u.LAST_ARCHIE_STATE is not null)
						)
	if @ck = 1
	BEGIN --all is well
		--1. Save Archie keys in tb_contact
		update TB_CONTACT set 
			ARCHIE_ID = au.ARCHIE_ID
			,ARCHIE_ACCESS_TOKEN = au.ARCHIE_ACCESS_TOKEN
			,ARCHIE_TOKEN_VALID_UNTIL = au.ARCHIE_TOKEN_VALID_UNTIL
			,ARCHIE_REFRESH_TOKEN = au.ARCHIE_REFRESH_TOKEN
			,LAST_ARCHIE_CODE = au.LAST_ARCHIE_CODE
			,LAST_ARCHIE_STATE = au.LAST_ARCHIE_STATE
		From (
				Select * from TB_UNASSIGNED_ARCHIE_KEYS u where @ARCHIE_CODE = u.LAST_ARCHIE_CODE 
				and @ARCHIE_STATE = u.LAST_ARCHIE_STATE 
				and (u.LAST_ARCHIE_CODE is not null and u.LAST_ARCHIE_STATE is not null)
				) au
		WHERE CONTACT_ID = @CID
		--2. delete record from TB_UNASSIGNED_ARCHIE_KEYS
		delete from TB_UNASSIGNED_ARCHIE_KEYS where @ARCHIE_CODE = LAST_ARCHIE_CODE 
				and @ARCHIE_STATE = LAST_ARCHIE_STATE 
				and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
		--3. get All user details
		SELECT * from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
			and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
	END
END

GO
ALTER PROCEDURE [dbo].[st_ArchieIdentityFromCodeAndStatus]
	-- Add the parameters for the stored procedure here
	@ARCHIE_CODE varchar(4000)
	,@ARCHIE_STATE varchar(12)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    declare @ck int = (Select COUNT(CONTACT_ID) from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
		and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null))
	if @ck = 1
	BEGIN
	SELECT * from TB_CONTACT where @ARCHIE_CODE = LAST_ARCHIE_CODE and @ARCHIE_STATE = LAST_ARCHIE_STATE 
		and (LAST_ARCHIE_CODE is not null and LAST_ARCHIE_STATE is not null)
	END
END
GO


