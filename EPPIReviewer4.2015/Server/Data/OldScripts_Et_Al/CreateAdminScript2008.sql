USE [ReviewerAdmin]
GO
/****** Object:  User [IOE\SSRU_DEV]    Script Date: 04/06/2010 17:52:02 ******/
CREATE USER [IOE\SSRU_DEV] FOR LOGIN [IOE\SSRU_DEV]
GO
/****** Object:  User [IOE\epi2$]    Script Date: 04/06/2010 17:52:02 ******/
CREATE USER [IOE\epi2$] FOR LOGIN [IOE\epi2$] WITH DEFAULT_SCHEMA=[db_owner]
GO
/****** Object:  Table [dbo].[TB_LOGON_TICKET]    Script Date: 04/06/2010 17:52:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_LOGON_TICKET](
	[TICKET_GUID] [uniqueidentifier] NOT NULL,
	[CONTACT_ID] [int] NOT NULL,
	[REVIEW_ID] [int] NOT NULL,
	[CREATED] [datetime2](0) NOT NULL,
	[LAST_RENEWED] [datetime2](0) NOT NULL,
	[STATE] [bit] NOT NULL,
 CONSTRAINT [PK_TB_LOGON_TICKET] PRIMARY KEY CLUSTERED 
(
	[TICKET_GUID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TB_LATEST_SERVER_MESSAGE]    Script Date: 04/06/2010 17:52:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TB_LATEST_SERVER_MESSAGE](
	[MESSAGE] [nvarchar](100) NOT NULL,
	[INSERT_TIME] [datetime2](0) NOT NULL
) ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [IX_TB_LATEST_SERVER_MESSAGE] ON [dbo].[TB_LATEST_SERVER_MESSAGE] 
(
	[INSERT_TIME] DESC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 04/06/2010 17:52:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- Create date: <24/03/10>
-- Description:	<gets contact details when loging in>
-- =============================================
CREATE PROCEDURE [dbo].[st_ContactLogin] 
(
	@USERNAME nvarchar(50),
	@PASSWORD nvarchar(50),
	@IP_ADDRESS nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT Reviewer.dbo.TB_CONTACT.* FROM Reviewer.dbo.TB_CONTACT
		WHERE Reviewer.dbo.TB_CONTACT.USERNAME = @USERNAME
		AND Reviewer.dbo.TB_CONTACT.[PASSWORD] = @PASSWORD
	
	RETURN
END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 04/06/2010 17:52:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[st_ContactDetails] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, c.[EXPIRY_DATE], c.MONTHS_CREDIT, c.CREATOR_ID,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN
	from Reviewer.dbo.TB_CONTACT c
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on c.CONTACT_ID = lt.CONTACT_ID
	where c.CONTACT_ID = @CONTACT_ID
	
	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN

	RETURN

END
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Insert]    Script Date: 04/06/2010 17:52:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 
-- Description:	create a new ticket, relies on default values of most of the columns
-- =============================================
CREATE PROCEDURE [dbo].[st_LogonTicket_Insert] 
	-- Add the parameters for the stored procedure here
	@Contact_ID int = 0, 
	@Review_ID int = 0,
	@GUID uniqueidentifier OUTPUT 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
    set @GUID = newid()
    UPDATE TB_LOGON_TICKET
    SET STATE = 0
    WHERE CONTACT_ID = @Contact_ID AND STATE = 1
    INSERT into TB_LOGON_TICKET(TICKET_GUID, CONTACT_ID, REVIEW_ID)
	VALUES (@GUID, @Contact_ID, @Review_ID)
	
END
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_RI]    Script Date: 04/06/2010 17:52:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Sergio>
-- Create date: <05/03/10>
-- Description:	<Updates LAST_RENEWED on a valid ticket, used to make sure only one user is logged with the same credentials and to renew a ticket validity>
-- =============================================
CREATE PROCEDURE [dbo].[st_LogonTicket_Check_RI] 
	-- Add the parameters for the stored procedure here
	@guid uniqueidentifier, 
	@c_ID int,
	@ROWS int OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	UPDATE TB_LOGON_TICKET SET LAST_RENEWED = GETDATE() 
	WHERE TICKET_GUID =@guid AND CONTACT_ID = @c_ID AND State=1
	set @ROWS = @@ROWCOUNT
END
GO
/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Expiration]    Script Date: 04/06/2010 17:52:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Sergio
-- Create date: 08/03/10
-- Description:	check ticket validity: checks if another more recent ticket is present and if current ticket is expired
-- Description: if ticket is valid retrieves also the latest message from the server
-- =============================================
CREATE PROCEDURE [dbo].[st_LogonTicket_Check_Expiration] 
	-- Add the parameters for the stored procedure here
	@guid uniqueidentifier, 
	@c_ID int,
	@result nvarchar(9) OUTPUT,
	@message nvarchar(100) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	DECLARE @RN_tm datetime2(0), @RC int
	DECLARE @myT TABLE 
( 
    RN datetime2(0),
    TK uniqueidentifier
) 
    set @message = ''
    -- to check for all possible situations, we get all valid tickets for user, ignoring the GUID
	insert into @myT 
		SELECT LAST_RENEWED, TICKET_GUID from TB_LOGON_TICKET WHERE CONTACT_ID = @c_ID AND State=1
	SET @RC = @@ROWCOUNT
	IF @RC = 0 --user does not have any valid ticket
	BEGIN
		SET @result = 'None'
	END
	
	ELSE IF @RC = 1 --just one found, so far so good!
	Begin
		IF @guid = (SELECT TK from @myT) --is the ticket the one the user sent us? (check GUID)
		BEGIN --the GUID is the right one, let's see if it's still valid (= not expired)
			--SET @RN_tm = 
			IF (SELECT RN from @myT) > DATEADD(HH, -3, GETDATE())
			BEGIN
				SET @result = 'Valid' --all is well, get latest message from server
				SET @message = (SELECT top 1 MESSAGE from TB_LATEST_SERVER_MESSAGE)
			END
			ELSE
			BEGIN
				SET @result = 'Expired' 
				--ticket is too old, set ticket state to FALSE
				UPDATE TB_LOGON_TICKET SET State=0 WHERE TICKET_GUID = @guid
			END
		END
		ELSE
		BEGIN
		--GUID didn't match, same credentials have been used to log on somewhere else
			SET @result = 'Invalid'
		END
	END
	ELSE IF @RC > 1 --for some reason, more than one valid ticket exist for current user
	BEGIN
		-- this shouldn't happen: invalidate ALL tickets
		UPDATE TB_LOGON_TICKET SET State=0
		WHERE CONTACT_ID = @c_ID AND State=1
		SET @result = 'Multiple'
	END
END
GO
/****** Object:  StoredProcedure [dbo].[st_ContactReviews]    Script Date: 04/06/2010 17:52:04 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jeff>
-- Create date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE PROCEDURE [dbo].[st_ContactReviews] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
	select r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, max(lt.CREATED) as CREATED, 
	r.MONTHS_CREDIT, r.[EXPIRY_DATE]
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on r.REVIEW_ID = rc.REVIEW_ID
	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
	on lt.REVIEW_ID = rc.REVIEW_ID
	where rc.CONTACT_ID = @CONTACT_ID
	-- this next line restricts it to just the contactID. Remove this line and you get the last login overall (all reviewers)
	and (lt.CONTACT_ID = @CONTACT_ID or lt.CONTACT_ID is null)
	
	group by r.REVIEW_ID, r.REVIEW_NAME, 
	r.DATE_CREATED, r.MONTHS_CREDIT, r.[EXPIRY_DATE]
	order by r.REVIEW_ID

END
GO
/****** Object:  Default [DF_TB_LOGON_TICKET_TICKET_GUID]    Script Date: 04/06/2010 17:52:03 ******/
ALTER TABLE [dbo].[TB_LOGON_TICKET] ADD  CONSTRAINT [DF_TB_LOGON_TICKET_TICKET_GUID]  DEFAULT (newid()) FOR [TICKET_GUID]
GO
/****** Object:  Default [DF_TB_LOGON_TICKET_CREATED]    Script Date: 04/06/2010 17:52:03 ******/
ALTER TABLE [dbo].[TB_LOGON_TICKET] ADD  CONSTRAINT [DF_TB_LOGON_TICKET_CREATED]  DEFAULT (getdate()) FOR [CREATED]
GO
/****** Object:  Default [DF_TB_LOGON_TICKET_LAST_RENEWED]    Script Date: 04/06/2010 17:52:03 ******/
ALTER TABLE [dbo].[TB_LOGON_TICKET] ADD  CONSTRAINT [DF_TB_LOGON_TICKET_LAST_RENEWED]  DEFAULT (getdate()) FOR [LAST_RENEWED]
GO
/****** Object:  Default [DF_TB_LOGON_TICKET_STATE]    Script Date: 04/06/2010 17:52:03 ******/
ALTER TABLE [dbo].[TB_LOGON_TICKET] ADD  CONSTRAINT [DF_TB_LOGON_TICKET_STATE]  DEFAULT ((1)) FOR [STATE]
GO
/****** Object:  Default [DF_TB_LATEST_SERVER_MESSAGE_INSERT_TIME]    Script Date: 04/06/2010 17:52:03 ******/
ALTER TABLE [dbo].[TB_LATEST_SERVER_MESSAGE] ADD  CONSTRAINT [DF_TB_LATEST_SERVER_MESSAGE_INSERT_TIME]  DEFAULT (getdate()) FOR [INSERT_TIME]
GO
