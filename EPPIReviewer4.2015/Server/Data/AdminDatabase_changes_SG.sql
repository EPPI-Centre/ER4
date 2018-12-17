--USE [ReviewerAdmin]
--GO

--/****** Object:  Table [dbo].[TB_UPDATE_MSG]    Script Date: 10/03/2014 11:10:35 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE TABLE [dbo].[TB_UPDATE_MSG](
--	[UPDATE_MSG_ID] [int] NOT NULL
--) ON [PRIMARY]

--GO

--/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
--BEGIN TRANSACTION
--SET QUOTED_IDENTIFIER ON
--SET ARITHABORT ON
--SET NUMERIC_ROUNDABORT OFF
--SET CONCAT_NULL_YIELDS_NULL ON
--SET ANSI_NULLS ON
--SET ANSI_PADDING ON
--SET ANSI_WARNINGS ON
--COMMIT
--BEGIN TRANSACTION
--GO
--CREATE TABLE dbo.Tmp_TB_UPDATE_MSG
--	(
--	UPDATE_MSG_ID int NOT NULL IDENTITY (1, 1),
--	VERSION_NUMBER nvarchar(15) NOT NULL,
--	DESCRIPTION nvarchar(500) NOT NULL,
--	URL nvarchar(160) NOT NULL,
--	DATE date NOT NULL
--	)  ON [PRIMARY]
--GO
--ALTER TABLE dbo.Tmp_TB_UPDATE_MSG SET (LOCK_ESCALATION = TABLE)
--GO
--ALTER TABLE dbo.Tmp_TB_UPDATE_MSG ADD CONSTRAINT
--	DF_TB_UPDATE_MSG_DATE DEFAULT (getdate()) FOR DATE
--GO
--SET IDENTITY_INSERT dbo.Tmp_TB_UPDATE_MSG ON
--GO
--IF EXISTS(SELECT * FROM dbo.TB_UPDATE_MSG)
--	 EXEC('INSERT INTO dbo.Tmp_TB_UPDATE_MSG (UPDATE_MSG_ID)
--		SELECT UPDATE_MSG_ID FROM dbo.TB_UPDATE_MSG WITH (HOLDLOCK TABLOCKX)')
--GO
--SET IDENTITY_INSERT dbo.Tmp_TB_UPDATE_MSG OFF
--GO
--DROP TABLE dbo.TB_UPDATE_MSG
--GO
--EXECUTE sp_rename N'dbo.Tmp_TB_UPDATE_MSG', N'TB_UPDATE_MSG', 'OBJECT' 
--GO
--ALTER TABLE dbo.TB_UPDATE_MSG ADD CONSTRAINT
--	PK_TB_UPDATE_MSG PRIMARY KEY CLUSTERED 
--	(
--	UPDATE_MSG_ID
--	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

--GO
--COMMIT
--GO
--INSERT INTO [TB_UPDATE_MSG]
--           (
--           [VERSION_NUMBER]
--           ,[DESCRIPTION]
--           ,[URL]
--           )
--     VALUES
--           ('4.4.1.0'
--           ,'This release includes a mix of new features and bug fixes. New features include: Risk of Bias reports, added support for reference DOI and Keywords and a new "Safe Upload" option for importing items.'
--           ,'http://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2932&forumid=22&threadid=1259&scope=posts'
--           )
--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_GetLatestUpdateMsg]    Script Date: 10/03/2014 11:56:39 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GetLatestUpdateMsg]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_GetLatestUpdateMsg]
--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_GetLatestUpdateMsg]    Script Date: 10/03/2014 11:56:39 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_GetLatestUpdateMsg] 
--	-- Add the parameters for the stored procedure here
	
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	Select top 1 * from TB_UPDATE_MSG 
--	order by UPDATE_MSG_ID desc
--END

--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_InsertLatestUpdateMsg]    Script Date: 10/03/2014 13:52:26 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_InsertLatestUpdateMsg]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_InsertLatestUpdateMsg]
--GO

--USE [ReviewerAdmin]
--GO

--/****** Object:  StoredProcedure [dbo].[st_InsertLatestUpdateMsg]    Script Date: 10/03/2014 13:52:26 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_InsertLatestUpdateMsg]
--	-- Add the parameters for the stored procedure here
--	@Ver nvarchar(15) 
--	,@Description nvarchar(500) 
--	,@url nvarchar(160)
--	,@cid int 
	
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    --first, check that the contact id provided is a siteadmin (small additional precaution to make mischief a little more difficult)
--    declare @chk int = 0
--    set @chk = (select COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @cid and IS_SITE_ADMIN = 1)
--    if @chk != 1 return 
    
--	INSERT INTO [TB_UPDATE_MSG]
--           (
--           [VERSION_NUMBER]
--           ,[DESCRIPTION]
--           ,[URL]
--           )
--     VALUES
--           (@Ver
--           ,@Description
--           ,@url
--           )
--END

--GO




----USE [ReviewerAdmin]
----GO

----/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Get_UserID_FROM_GUID]    Script Date: 09/07/2010 16:42:03 ******/
----IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_LogonTicket_Get_UserID_FROM_GUID]') AND type in (N'P', N'PC'))
----DROP PROCEDURE [dbo].[st_LogonTicket_Get_UserID_FROM_GUID]
----GO

----USE [ReviewerAdmin]
----GO

----/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Get_UserID_FROM_GUID]    Script Date: 09/07/2010 16:42:03 ******/
----SET ANSI_NULLS ON
----GO

----SET QUOTED_IDENTIFIER ON
----GO

------ =============================================
------ Author:		<Sergio>
------ Create date: <05/03/10>
------ Description:	
------ =============================================
----CREATE PROCEDURE [dbo].[st_LogonTicket_Get_UserID_FROM_GUID] 
----	-- Add the parameters for the stored procedure here
----	@guid uniqueidentifier 

----AS
----BEGIN
----	-- SET NOCOUNT ON added to prevent extra result sets from
----	-- interfering with SELECT statements.
----	SET NOCOUNT ON;
----	declare @ROWS int
----    -- Insert statements for procedure here
----	UPDATE TB_LOGON_TICKET SET LAST_RENEWED = GETDATE() 
----	WHERE TICKET_GUID =@guid AND State=1
----	set @ROWS = @@ROWCOUNT
----	if @ROWS = 1
----	Begin
----		Select CONTACT_ID, Review_id from TB_LOGON_TICKET WHERE TICKET_GUID =@guid AND State=1
----	END	
----END

----GO



------USE [ReviewerAdmin]
------GO
------/****** Object:  StoredProcedure [dbo].[st_DNNContactDetails]    Script Date: 06/10/2010 11:47:28 ******/
------SET ANSI_NULLS ON
------GO
------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		<Author,,Name>
-------- Create date: <Create Date,,>
-------- Description:	<Description,,>
-------- =============================================
------CREATE PROCEDURE [dbo].[st_DNNContactDetails] 
------(
------	@Username nvarchar(50)
------)
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;

------    -- Insert statements for procedure here
------    select top 1 c.CONTACT_ID
------    , c.old_contact_id
------    , c.CONTACT_NAME
------    , c.USERNAME
------    --, c.[PASSWORD]
------    , c.EMAIL
------    , lt.CREATED as LAST_LOGIN
------    , lt.LAST_RENEWED as LAST_ACTIVITY
------    , c.DATE_CREATED
------    , c.[EXPIRY_DATE]
------    --, c.MONTHS_CREDIT
------    --, c.CREATOR_ID
------    , c.[TYPE]
------    , c.IS_SITE_ADMIN
------	from Reviewer.dbo.TB_CONTACT c
------	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
------	on c.CONTACT_ID = lt.CONTACT_ID
------	where c.USERNAME = @Username
------	Order by LAST_ACTIVITY

------	RETURN

------END

------/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
------BEGIN TRANSACTION
------SET QUOTED_IDENTIFIER ON
------SET ARITHABORT ON
------SET NUMERIC_ROUNDABORT OFF
------SET CONCAT_NULL_YIELDS_NULL ON
------SET ANSI_NULLS ON
------SET ANSI_PADDING ON
------SET ANSI_WARNINGS ON
------COMMIT
------BEGIN TRANSACTION
------GO
------ALTER TABLE dbo.TB_LATEST_SERVER_MESSAGE
------	DROP CONSTRAINT DF_TB_LATEST_SERVER_MESSAGE_INSERT_TIME
------GO
------CREATE TABLE dbo.Tmp_TB_LATEST_SERVER_MESSAGE
------	(
------	MESSAGE nvarchar(4000) NOT NULL,
------	INSERT_TIME datetime2(0) NOT NULL
------	)  ON [PRIMARY]
------GO
------ALTER TABLE dbo.Tmp_TB_LATEST_SERVER_MESSAGE SET (LOCK_ESCALATION = TABLE)
------GO
------ALTER TABLE dbo.Tmp_TB_LATEST_SERVER_MESSAGE ADD CONSTRAINT
------	DF_TB_LATEST_SERVER_MESSAGE_INSERT_TIME DEFAULT (getdate()) FOR INSERT_TIME
------GO
------IF EXISTS(SELECT * FROM dbo.TB_LATEST_SERVER_MESSAGE)
------	 EXEC('INSERT INTO dbo.Tmp_TB_LATEST_SERVER_MESSAGE (MESSAGE, INSERT_TIME)
------		SELECT MESSAGE, INSERT_TIME FROM dbo.TB_LATEST_SERVER_MESSAGE WITH (HOLDLOCK TABLOCKX)')
------GO
------DROP TABLE dbo.TB_LATEST_SERVER_MESSAGE
------GO
------EXECUTE sp_rename N'dbo.Tmp_TB_LATEST_SERVER_MESSAGE', N'TB_LATEST_SERVER_MESSAGE', 'OBJECT' 
------GO
------CREATE CLUSTERED INDEX IX_TB_LATEST_SERVER_MESSAGE ON dbo.TB_LATEST_SERVER_MESSAGE
------	(
------	INSERT_TIME DESC
------	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
------GO
------COMMIT



------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Expiration]    Script Date: 07/07/2010 16:37:24 ******/
------IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_LogonTicket_Check_Expiration]') AND type in (N'P', N'PC'))
------DROP PROCEDURE [dbo].[st_LogonTicket_Check_Expiration]
------GO

------USE [ReviewerAdmin]
------GO

------/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Expiration]    Script Date: 07/07/2010 16:37:24 ******/
------SET ANSI_NULLS ON
------GO

------SET QUOTED_IDENTIFIER ON
------GO

-------- =============================================
-------- Author:		Sergio
-------- Create date: 08/03/10
-------- Description:	check ticket validity: checks if another more recent ticket is present and if current ticket is expired
-------- Description: if ticket is valid retrieves also the latest message from the server
-------- =============================================
------CREATE PROCEDURE [dbo].[st_LogonTicket_Check_Expiration] 
------	-- Add the parameters for the stored procedure here
------	@guid uniqueidentifier, 
------	@c_ID int,
------	@result nvarchar(9) OUTPUT,
------	@message nvarchar(4000) OUTPUT
------AS
------BEGIN
------	-- SET NOCOUNT ON added to prevent extra result sets from
------	-- interfering with SELECT statements.
------	SET NOCOUNT ON;
------	DECLARE @RN_tm datetime2(0), @RC int
------	DECLARE @myT TABLE 
------( 
------    RN datetime2(0),
------    TK uniqueidentifier
------) 
------    set @message = ''
------    -- to check for all possible situations, we get all valid tickets for user, ignoring the GUID
------	insert into @myT 
------		SELECT LAST_RENEWED, TICKET_GUID from TB_LOGON_TICKET WHERE CONTACT_ID = @c_ID AND State=1
------	SET @RC = @@ROWCOUNT
------	IF @RC = 0 --user does not have any valid ticket
------	BEGIN
------		SET @result = 'None'
------	END
	
------	ELSE IF @RC = 1 --just one found, so far so good!
------	Begin
------		IF @guid = (SELECT TK from @myT) --is the ticket the one the user sent us? (check GUID)
------		BEGIN --the GUID is the right one, let's see if it's still valid (= not expired)
------			--SET @RN_tm = 
------			IF (SELECT RN from @myT) > DATEADD(HH, -3, GETDATE())
------			BEGIN
------				SET @result = 'Valid' --all is well, get latest message from server
------				SET @message = (SELECT top 1 MESSAGE from TB_LATEST_SERVER_MESSAGE)
------			END
------			ELSE
------			BEGIN
------				SET @result = 'Expired' 
------				--ticket is too old, set ticket state to FALSE
------				UPDATE TB_LOGON_TICKET SET State=0 WHERE TICKET_GUID = @guid
------			END
------		END
------		ELSE
------		BEGIN
------		--GUID didn't match, same credentials have been used to log on somewhere else
------			SET @result = 'Invalid'
------		END
------	END
------	ELSE IF @RC > 1 --for some reason, more than one valid ticket exist for current user
------	BEGIN
------		-- this shouldn't happen: invalidate ALL tickets
------		UPDATE TB_LOGON_TICKET SET State=0
------		WHERE CONTACT_ID = @c_ID AND State=1
------		SET @result = 'Multiple'
------	END
------END

------GO

