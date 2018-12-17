----THIS script is DANGEROUS!!!!!!!!!!!!!!!!!!!!!!!!!!!

----To note:
----1) The USE [Database] commands are minimised, only one per DB
----2) If used against [TestReviewer...] you should uncomment lines 490+ to add the expected data in tb_for_sale 
----	 DOING the same agains the live DB will be a DISASTER!!!!!!!!!!!! (info on Site License prices will be gone!)



--use [Reviewer]
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
--ALTER TABLE dbo.TB_CONTACT ADD
--	FLAVOUR char(20) NULL,
--	PWASHED binary(20) NULL
--GO
--ALTER TABLE dbo.TB_CONTACT SET (LOCK_ESCALATION = TABLE)
--GO
--COMMIT
--GO


----write all hashes
--DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
--declare @rnd varchar(20) = str(rand(),17,15)
--select @rnd, LEN (@rnd)

--declare @t table (line int, salt char(20), orig varchar(50), AccountPwd varbinary(20)  )
--declare @cnt int = 0, @cc int = 1
--declare @pw varchar(50) = null
--while (@cc <7500) -- doing this across all 5000- accounts and more for good measure.
--BEGIN
--	set @cnt = 0
--	set @rnd = ''
--	select @pw = (select password from TB_CONTACT where CONTACT_ID = @cc and [PASSWORD] != 'FROZEN_ACCOUNT')
--	if (@pw is not null)
--	BEGIN 
--		WHILE (@cnt <= 20) 
--		BEGIN
--			SELECT @rnd = @rnd + 
--				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
--			SELECT @cnt = @cnt + 1
--		END
--		insert into @t select @cc, @rnd, @pw, HASHBYTES('SHA1', @pw + @rnd)
--	END
--	SELECT @cc = @cc + 1
--END
--select * from @t
--update c
--set c.FLAVOUR = t.salt, c.PWASHED = t.AccountPwd
--from TB_CONTACT c
--    inner join @t t on t.line = c.CONTACT_ID and t.orig = c.PASSWORD
    
--select *,[password] from TB_CONTACT
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 12/18/2014 10:50:34 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--ALTER procedure [dbo].[st_ContactLogin]
--(
--	@userName  varchar(50)	
--	,@Password varchar(50)
	
--)
----note the GRACE_EXP field, how many days we add to EXPIRY_DATE defines how long is the grace period for the whole of ER4.
----during the grace period users can log on ER4 but will have read only access.
--As


----first check if the username/pw are correct
--declare @chek int = (select count(Contact_id)  from TB_CONTACT c where c.USERNAME = @userName and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED AND c.EXPIRY_DATE is not null)
--if @chek = 1

----second get some user info
--BEGIN
--	Select c.CONTACT_ID, c.contact_name, --c.Password, 
--		DATEADD(m, 2, 
--				( CASE when sl.[EXPIRY_DATE] is not null 
--					and sl.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--						then sl.[EXPIRY_DATE]
--					else c.[EXPIRY_DATE]
--					end
--				)) as GRACE_EXP,
--		[TYPE], IS_SITE_ADMIN  /* TB_CONTACT.[Role] */
--	From TB_CONTACT c
--	Left outer join TB_SITE_LIC_CONTACT lc on lc.CONTACT_ID = c.CONTACT_ID
--	Left outer join TB_SITE_LIC sl on lc.SITE_LIC_ID = sl.SITE_LIC_ID
--	Where c.UserName = @userName and c.EXPIRY_DATE is not null
--END
--GO

--USE [ReviewerAdmin]
--GO

--IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_TB_CHECKLINK_IS_STALE]') AND parent_object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK]'))
--Begin
--IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_CHECKLINK_IS_STALE]') AND type = 'D')
--BEGIN
--ALTER TABLE [dbo].[TB_CHECKLINK] DROP CONSTRAINT [DF_TB_CHECKLINK_IS_STALE]
--END


--End
--GO
--IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_TB_CHECKLINK_LOG_ALREADY_SENT]') AND parent_object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK_LOG]'))
--Begin
--IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_CHECKLINK_LOG_ALREADY_SENT]') AND type = 'D')
--BEGIN
--ALTER TABLE [dbo].[TB_CHECKLINK_LOG] DROP CONSTRAINT [DF_TB_CHECKLINK_LOG_ALREADY_SENT]
--END


--End
--GO
--IF  EXISTS (SELECT * FROM sys.default_constraints WHERE object_id = OBJECT_ID(N'[dbo].[DF_TB_CHECKLINK_LOG_DATE_CREATED]') AND parent_object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK_LOG]'))
--Begin
--IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_TB_CHECKLINK_LOG_DATE_CREATED]') AND type = 'D')
--BEGIN
--ALTER TABLE [dbo].[TB_CHECKLINK_LOG] DROP CONSTRAINT [DF_TB_CHECKLINK_LOG_DATE_CREATED]
--END


--End
--GO
--/****** Object:  StoredProcedure [dbo].[st_GhostContactActivate]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GhostContactActivate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_GhostContactActivate]
--GO
--/****** Object:  StoredProcedure [dbo].[st_TransferAccountCredit]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_TransferAccountCredit]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_TransferAccountCredit]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactActivate]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactActivate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactActivate]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactCheckUnameOrEmail]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactCheckUnameOrEmail]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactCheckUnameOrEmail]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactCreate]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactCreate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactCreate]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactDetails]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactDetails]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactEdit]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactEdit]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactEdit]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactLogin]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactLogin]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedDetails]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactPurchasedDetails]
--GO
--/****** Object:  StoredProcedure [dbo].[st_EmailUpdate]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_EmailUpdate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_EmailUpdate]
--GO
--/****** Object:  StoredProcedure [dbo].[st_GhostContactActivateRevoke]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GhostContactActivateRevoke]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_GhostContactActivateRevoke]
--GO
--/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Ticket]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_LogonTicket_Check_Ticket]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_LogonTicket_Check_Ticket]
--GO
--/****** Object:  StoredProcedure [dbo].[st_ResetPassword]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ResetPassword]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ResetPassword]
--GO
--/****** Object:  StoredProcedure [dbo].[st_CheckGhostAccountBeforeActivation]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckGhostAccountBeforeActivation]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckGhostAccountBeforeActivation]
--GO
--/****** Object:  StoredProcedure [dbo].[st_CheckLinkAddFailureLog]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckLinkAddFailureLog]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckLinkAddFailureLog]
--GO
--/****** Object:  StoredProcedure [dbo].[st_CheckLinkCheck]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckLinkCheck]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckLinkCheck]
--GO
--/****** Object:  StoredProcedure [dbo].[st_CheckLinkCreate]    Script Date: 01/14/2015 15:51:43 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckLinkCreate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckLinkCreate]
--GO
--/****** Object:  Table [dbo].[TB_FOR_SALE]    Script Date: 01/14/2015 15:51:44 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_FOR_SALE]') AND type in (N'U'))
--DROP TABLE [dbo].[TB_FOR_SALE]
--GO
--/****** Object:  Table [dbo].[TB_MANAGMENT_EMAILS]    Script Date: 01/14/2015 15:51:44 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_MANAGMENT_EMAILS]') AND type in (N'U'))
--DROP TABLE [dbo].[TB_MANAGMENT_EMAILS]
--GO
--/****** Object:  Table [dbo].[TB_CHECKLINK]    Script Date: 01/14/2015 15:51:44 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK]') AND type in (N'U'))
--DROP TABLE [dbo].[TB_CHECKLINK]
--GO
--/****** Object:  Table [dbo].[TB_CHECKLINK_LOG]    Script Date: 01/14/2015 15:51:44 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK_LOG]') AND type in (N'U'))
--DROP TABLE [dbo].[TB_CHECKLINK_LOG]
--GO
--/****** Object:  Table [dbo].[TB_CHECKLINK_LOG]    Script Date: 01/14/2015 15:51:44 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--SET ANSI_PADDING ON
--GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK_LOG]') AND type in (N'U'))
--BEGIN
--CREATE TABLE [dbo].[TB_CHECKLINK_LOG](
--	[CHECKLINK_LOG_ID] [int] IDENTITY(1,1) NOT NULL,
--	[CHECKLINK_UID] [uniqueidentifier] NULL,
--	[TYPE] [varchar](15) NULL,
--	[QUERY_STRING] [nvarchar](1000) NULL,
--	[CONTACT_ID] [int] NULL,
--	[FAILURE_REASON] [nchar](500) NOT NULL,
--	[ALREADY_SENT] [bit] NOT NULL,
--	[DATE_CREATED] [datetime2](1) NOT NULL,
--	[IP_B1] [tinyint] NOT NULL,
--	[IP_B2] [tinyint] NOT NULL,
--	[IP_B3] [tinyint] NOT NULL,
--	[IP_B4] [tinyint] NOT NULL,
-- CONSTRAINT [PK_TB_CHECKLINK_LOG] PRIMARY KEY CLUSTERED 
--(
--	[CHECKLINK_LOG_ID] ASC
--)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--) ON [PRIMARY]
--END
--GO
--SET ANSI_PADDING OFF
--GO
--SET IDENTITY_INSERT [dbo].[TB_CHECKLINK_LOG] ON
--INSERT [dbo].[TB_CHECKLINK_LOG] ([CHECKLINK_LOG_ID], [CHECKLINK_UID], [TYPE], [QUERY_STRING], [CONTACT_ID], [FAILURE_REASON], [ALREADY_SENT], [DATE_CREATED], [IP_B1], [IP_B2], [IP_B3], [IP_B4]) VALUES (1, NULL, NULL, N'LUID=269fd8fb-bb84-4a62-a41c-634103a5eb23&CID=1214', 0, N'RESET PASSWORD Error: Failed to validate link and/or account info when trying to acutally change the PW                                                                                                                                                                                                                                                                                                                                                                                                             ', 1, CAST(0x018883064C390B0000 AS DateTime2), 127, 0, 0, 1)
--INSERT [dbo].[TB_CHECKLINK_LOG] ([CHECKLINK_LOG_ID], [CHECKLINK_UID], [TYPE], [QUERY_STRING], [CONTACT_ID], [FAILURE_REASON], [ALREADY_SENT], [DATE_CREATED], [IP_B1], [IP_B2], [IP_B3], [IP_B4]) VALUES (2, N'b0a0f846-26a0-45c2-b3aa-b6a0e8827659', N'Not found', N'LUID=b0a0f846-26a0-45c2-b3aa-b6a0e8827659&CID=1214', 1214, N'Invalid query values: combination of CID and UID was not found, or link is stale                                                                                                                                                                                                                                                                                                                                                                                                                                    ', 1, CAST(0x013DBF054F390B0000 AS DateTime2), 127, 0, 0, 1)
--SET IDENTITY_INSERT [dbo].[TB_CHECKLINK_LOG] OFF
--/****** Object:  Table [dbo].[TB_CHECKLINK]    Script Date: 01/14/2015 15:51:44 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--SET ANSI_PADDING ON
--GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_CHECKLINK]') AND type in (N'U'))
--BEGIN
--CREATE TABLE [dbo].[TB_CHECKLINK](
--	[CHECKLINK_ID] [int] IDENTITY(1,1) NOT NULL,
--	[CHECKLINK_UID] [uniqueidentifier] NOT NULL,
--	[TYPE] [varchar](15) NOT NULL,
--	[DATE_CREATED] [datetime2](1) NOT NULL,
--	[CONTACT_ID] [int] NOT NULL,
--	[IS_STALE] [bit] NOT NULL,
--	[DATE_USED] [datetime2](1) NULL,
--	[WAS_SUCCESS] [bit] NULL,
--	[CC_EMAIL] [nvarchar](500) NULL,
-- CONSTRAINT [PK_TB_CHECKLINK] PRIMARY KEY CLUSTERED 
--(
--	[CHECKLINK_ID] ASC
--)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--) ON [PRIMARY]
--END
--GO
--SET ANSI_PADDING OFF
--GO
--SET IDENTITY_INSERT [dbo].[TB_CHECKLINK] ON
--INSERT [dbo].[TB_CHECKLINK] ([CHECKLINK_ID], [CHECKLINK_UID], [TYPE], [DATE_CREATED], [CONTACT_ID], [IS_STALE], [DATE_USED], [WAS_SUCCESS], [CC_EMAIL]) VALUES (1, N'ab91fa25-d08e-4da0-a0ad-2aa16dc2d3a9', N'test', CAST(0x019BEA0842390B0000 AS DateTime2), 1214, 0, NULL, NULL, N'test@test.com')
--INSERT [dbo].[TB_CHECKLINK] ([CHECKLINK_ID], [CHECKLINK_UID], [TYPE], [DATE_CREATED], [CONTACT_ID], [IS_STALE], [DATE_USED], [WAS_SUCCESS], [CC_EMAIL]) VALUES (2, N'a3db0b2d-1e98-4491-8db3-a257a57785cd', N'CheckEmail', CAST(0x01CF460843390B0000 AS DateTime2), 1214, 1, CAST(0x01B2350943390B0000 AS DateTime2), 1, NULL)
--INSERT [dbo].[TB_CHECKLINK] ([CHECKLINK_ID], [CHECKLINK_UID], [TYPE], [DATE_CREATED], [CONTACT_ID], [IS_STALE], [DATE_USED], [WAS_SUCCESS], [CC_EMAIL]) VALUES (3, N'c5f238a5-0900-4ff1-ac1b-7febf69b9db1', N'ResetPw', CAST(0x01BF460943390B0000 AS DateTime2), 1214, 1, CAST(0x01EC480943390B0000 AS DateTime2), NULL, NULL)
--SET IDENTITY_INSERT [dbo].[TB_CHECKLINK] OFF
--/****** Object:  Table [dbo].[TB_MANAGMENT_EMAILS]    Script Date: 01/14/2015 15:51:44 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_MANAGMENT_EMAILS]') AND type in (N'U'))
--BEGIN
--CREATE TABLE [dbo].[TB_MANAGMENT_EMAILS](
--	[EMAIL_ID] [int] IDENTITY(1,1) NOT NULL,
--	[EMAIL_NAME] [nvarchar](50) NOT NULL,
--	[EMAIL_MESSAGE] [nvarchar](4000) NULL,
-- CONSTRAINT [PK_TB_MANAGMENT_EMAILS] PRIMARY KEY CLUSTERED 
--(
--	[EMAIL_ID] ASC
--)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--) ON [PRIMARY]
--END
--GO
--SET IDENTITY_INSERT [dbo].[TB_MANAGMENT_EMAILS] ON
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (1, N'Welcome Message', N'<p>
--      EPPI-Reviewer 4: Software for research synthesis
--    </p>
--    <p>
--      Dear FullNameHere
--    </p>
      
--      Your EPPI-Reviewer 4 account is now ready for use.<br>
--      The account details are:<br>
--	  Username: "UsernameHere"<br>
--	  Valid until: ExpiryDateHere.<br>
--    <p>
--      You will find a small example review under your account when you first log into EPPI-Reviewer 4.<br> 
--	  You are free to browse and edit this example review to explore many of the programs functions. There is a video to accompany this example review on our YouTube channel (<a href="http://www.youtube.com/user/eppireviewer4"><font color="purple">http://www.youtube.com/user/eppireviewer4</font></a>).  The name of the video is "EPPI-Reviewer 4 example systematic review".
--    </p>
--    <p>
--      On the EPPI-Reviewer 4 gateway (<a href="http://eppi.ioe.ac.uk/cms/er4">http://eppi.ioe.ac.uk/cms/er4</a>) can be found details about your trial account, the user manual, the support forum, and links to the instructional videos on our YouTube channel.<br>
--      Also on the gateway, you’ll find the <a href="http://eppi.ioe.ac.uk/cms/Default.aspx?tabid=2935">Account and Review Manager<a/>. You can use the Account Manager to adjust your account details, change your password, manage your team, purchase subscriptions (for yourself and/or others) and more.
--	  <br>
--      Finally, you may also be receiving our EPPI-Reviewer newsletter that will keep you up to date on everything that is happening with the software.<br>
--      <br>
--      EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the UCL Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <a href="http://eppi.ioe.ac.uk/">eppi.ioe.ac.uk</a>.<br>
--      <br>
--      <br>
--      In case this message is unexpected, please don’t hesitate to contact our support staff:<br>
--      <a href="mailto:EPPIsupport@ioe.ac.uk">EPPIsupport@ioe.ac.uk</a><br>
--      <br>
--    </p>
--	AdminMessageHere<br>
--    <br>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (2, N'Forgotten password', N'<p>
--  EPPI-Reviewer 4: Software for research synthesis
--</p>
--<p>
--  Dear FullNameHere
--</p>
--<p>
--  AdminMessageHere
--</p>
--<p>
--  FullNameHere has requested a password reset message for EPPI-Reviewer 4.<br>
--  <table style="width:100%" border="0">
--	<!-- <tr><td style="background-color:#AADDEE">
--		<p>In order to use this account <span style="font-weight: 700; text-decoration: underline;">you must confirm your email address.</span> </p>
--	</td></tr> -->
--	<tr><td style="background-color:#FFEEAA"><p style="text-align: center; font-weight: 700;">To reset your password, please click:<br> <a href="linkURLhere">linkURLhere</a></p>
		
--	</td></tr>
--  </table>
--</p>
--<p>
--  The login page for EPPI-Reviewer 4 can be found at: <a href="http://eppi.ioe.ac.uk/eppireviewer4">http://eppi.ioe.ac.uk/eppireviewer4/</a>
--</p>
--<p>
--  On the EPPI-Reviewer 4 gateway (<a href="http://eppi.ioe.ac.uk/cms/er4">http://eppi.ioe.ac.uk/cms/er4</a>) can be found details about your user account, the user manual, the support forum, and links to instructional videos on our YouTube channel <a href="http://www.youtube.com/user/eppireviewer4">http://www.youtube.com/user/eppireviewer4</a>.
--</p>
--<p>
--  EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <a href="http://eppi.ioe.ac.uk/">eppi.ioe.ac.uk</a>.
--</p>
--<p>
--  <br>
--  In case this message is unexpected, please don"t hesitate to contact our support staff:<br>
--  <a href="mailto:EPPIsupport@ioe.ac.uk">EPPIsupport@ioe.ac.uk</a><br>
--</p>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (3, N'Review invitation', N'<P class=" ">EPPI-Reviewer 4: Software for research synthesis <BR><BR>Dear InviteeNameHere <BR><BR>InviterNameHere has invited you to be part of the review: ReviewNameHere <BR>The next time that you log into EPPI-Reviewer 4 you will see this review&nbsp;in your list of available reviews.<BR><BR>ProblemWithAccountMsgHere<BR><BR>The login page for EPPI-Reviewer 4 can be found at: <A href="http://eppi.ioe.ac.uk/eppireviewer4/">http://eppi.ioe.ac.uk/eppireviewer4/</A> <BR><BR>On the ''EPPI-Reviewer 4 gateway'' (<A href="http://eppi.ioe.ac.uk/cms/er4"><FONT color=#800080>http://eppi.ioe.ac.uk/cms/er4</FONT></A>) can be found details about your user account, the user manual, the support forum, and links to instructional videos on our YouTube channel <A href="http://www.youtube.com/user/eppireviewer4"><FONT color=#800080>http://www.youtube.com/user/eppireviewer4</FONT></A>.</P>
--<P>EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <A href="http://eppi.ioe.ac.uk/">eppi.ioe.ac.uk</A>. <BR><BR><BR>In case this message is unexpected, please don''t hesitate to contact our support staff:<BR><A href="mailto:EPPIsupport@ioe.ac.uk">EPPIsupport@ioe.ac.uk</A> <BR><BR></P>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (4, N'Ghost account activation', N'<p>
--      EPPI-Reviewer 4: Software for research synthesis<br>
--      <br>
--      Dear FullNameHere,<br>
--      <br>
--      PurcharserNameHere has bought an EPPI-Reviewer 4 account for you.<br>
--	  <table style="width:100%" border="0">
--		<tr><td style="background-color:#FFEEAA"><p style="text-align: center; font-weight: 700;">To activate your account, please click:<br> <a href="linkURLhere">linkURLhere</a></p>
--		</td></tr>
--	  </table>
--	  The link above will remain active for 14 days. After this period PurcharserNameHere will need to re-initiate the Account Activation procedure and a new email will be sent to you.
--    </p>
--    <p>
--      EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the UCL Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <a href="http://eppi.ioe.ac.uk/"><font color="purple">eppi.ioe.ac.uk</font></a>.<br>
--      <br>
--      <br>
--      In case this message is unexpected, please don’t hesitate to contact PurcharserNameHere and/or our support staff:<br>
--      <a href="mailto:EPPIsupport@ioe.ac.uk"><font color="#0066CC">EPPIsupport@ioe.ac.uk</font></a><br>
--      <br>
--    </p>
--	<br>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (5, N'Example review creation error', N'<P class=" ">EPPI-Reviewer 4: Software for research synthesis <BR><BR>Dear EPPISupport<BR><BR>An example review being created for ContactID ContactIDHere has generated an error message.<BR>The error message is: ErrorMessageHere</P>
--<P class=" ">Note: this message has been sent to EPPISupport only.<BR><BR></P>
--<P class=" ">EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <A href="http://eppi.ioe.ac.uk/"><FONT color=#800080>eppi.ioe.ac.uk</FONT></A>. <BR><BR>In case this message is unexpected, please don''t hesitate to contact our support staff:<BR><A href="mailto:EPPIsupport@ioe.ac.uk"><FONT color=#0066cc>EPPIsupport@ioe.ac.uk</FONT></A> <BR><BR></P>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (6, N'Forgotten Username', N'<p>
--      EPPI-Reviewer 4: Software for research synthesis<br>
--      <br>
--      Dear FullNameHere<br>
--      <br>
--      AdminMessageHere
--      FullNameHere has requested a Username reminder for EPPI-Reviewer 4.<br>
--	  <table style="width:100%" border="0">
--		<tr><td style="background-color:#FFEEAA"><p style="text-align: center;">The Username associated with this email address is:
--		<br><span style="font-weight: 700;">UsernameHere</span></p>
			
--		</td></tr>
--	  </table>
--    </p>
--    <p>
--      The login page for EPPI-Reviewer 4 can be found at: <a href="http://eppi.ioe.ac.uk/eppireviewer4/"><font color="purple">http://eppi.ioe.ac.uk/eppireviewer4/</font></a><br>
--      <br>
--      On the "EPPI-Reviewer 4 gateway" (<a href="http://eppi.ioe.ac.uk/cms/er4"><font color="purple">http://eppi.ioe.ac.uk/cms/er4</font></a>) can be found details about your user account, the user manual, the support forum, and links to instructional videos on our YouTube channel <a href="http://www.youtube.com/user/eppireviewer4"><font color="purple">http://www.youtube.com/user/eppireviewer4</font></a>.
--    </p>
--    <p>
--      EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <a href="http://eppi.ioe.ac.uk/"><font color="purple">eppi.ioe.ac.uk</font></a>.<br>
--      <br>
--      <br>
--      In case this message is unexpected, please don"t hesitate to contact our support staff:<br>
--      <a href="mailto:EPPIsupport@ioe.ac.uk"><font color="#0066CC">EPPIsupport@ioe.ac.uk</font></a><br>
--      <br>
--    </p>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (7, N'Activate New Account', N'<p>
--      EPPI-Reviewer 4: Software for research synthesis
--    </p>
--    <p>
--      Dear FullNameHere
--    </p>
      
--      An account for FullNameHere has been created in EPPI-Reviewer 4.<br>
--      <table style="width:100%" border="0">
--		<tr><td style="background-color:#FFEEAA"><p style="text-align: center; font-weight: 700;">To activate your account, please click:<br> <a href="linkURLhere">linkURLhere</a></p>
			
--		</td></tr>
--	  </table>
--    <p>
--      Once you have activated your account, the login page for EPPI-Reviewer 4 can be found at <a href="http://eppi.ioe.ac.uk/eppireviewer4/">http://eppi.ioe.ac.uk/eppireviewer4/</a>.<br>
--      <br>
--      In case this message is unexpected, please don’t hesitate to contact our support staff:<br>
--      <a href="mailto:EPPIsupport@ioe.ac.uk">EPPIsupport@ioe.ac.uk</a><br>
--      <br>
--    </p>
--	AdminMessageHere<br>
--    <br>')
--INSERT [dbo].[TB_MANAGMENT_EMAILS] ([EMAIL_ID], [EMAIL_NAME], [EMAIL_MESSAGE]) VALUES (8, N'Transfer Credit', N'<p>
--      EPPI-Reviewer 4: Software for research synthesis<br>
--      <br>
--      Dear FullNameHere,<br>
--      <br>
--      PurcharserNameHere has bought an EPPI-Reviewer 4 subscription for you.<br>
--	  Your account has received a MonthsCreditHere months renewal. Your account is now valid until <b>NewDateHere</b>.
--    </p>
--    <p>
--      EPPI-Reviewer 4 is developed and maintained by the EPPI-Centre of the UCL Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <a href="http://eppi.ioe.ac.uk/"><font color="purple">eppi.ioe.ac.uk</font></a>.<br>
--      <br>
--      <br>
--      In case this message is unexpected, please don’t hesitate to contact PurcharserNameHere and/or our support staff:<br>
--      <a href="mailto:EPPIsupport@ioe.ac.uk"><font color="#0066CC">EPPIsupport@ioe.ac.uk</font></a><br>
--      <br>
--    </p>
--	<br>')
--SET IDENTITY_INSERT [dbo].[TB_MANAGMENT_EMAILS] OFF
--/****** Object:  Table [dbo].[TB_FOR_SALE]    Script Date: 01/14/2015 15:51:44 ******/
--SET ANSI_NULLS ON
--GO
--SET QUOTED_IDENTIFIER ON
--GO
--IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TB_FOR_SALE]') AND type in (N'U'))
--BEGIN
--CREATE TABLE [dbo].[TB_FOR_SALE](
--	[FOR_SALE_ID] [int] IDENTITY(1,1) NOT NULL,
--	[TYPE_NAME] [nvarchar](50) NULL,
--	[IS_ACTIVE] [bit] NULL,
--	[LAST_CHANGED] [datetime] NULL,
--	[PRICE_PER_MONTH] [int] NULL,
--	[DETAILS] [nvarchar](max) NULL,
-- CONSTRAINT [PK_TB_FOR_SALE] PRIMARY KEY CLUSTERED 
--(
--	[FOR_SALE_ID] ASC
--)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
--) ON [PRIMARY]
--END
--GO


------Uncomment the next bit for testReviewerAdmin only!!
----SET IDENTITY_INSERT [dbo].[TB_FOR_SALE] ON
----INSERT [dbo].[TB_FOR_SALE] ([FOR_SALE_ID], [TYPE_NAME], [IS_ACTIVE], [LAST_CHANGED], [PRICE_PER_MONTH], [DETAILS]) VALUES (3, N'Professional', 1, CAST(0x00009DF200000000 AS DateTime), 10, N'Account')
----INSERT [dbo].[TB_FOR_SALE] ([FOR_SALE_ID], [TYPE_NAME], [IS_ACTIVE], [LAST_CHANGED], [PRICE_PER_MONTH], [DETAILS]) VALUES (4, N'Shareable Review', 1, CAST(0x00009DF200000000 AS DateTime), 35, N'Review')
----INSERT [dbo].[TB_FOR_SALE] ([FOR_SALE_ID], [TYPE_NAME], [IS_ACTIVE], [LAST_CHANGED], [PRICE_PER_MONTH], [DETAILS]) VALUES (6, N'Site License', 0, CAST(0x00009FDB00C61512 AS DateTime), 200, N'')
----INSERT [dbo].[TB_FOR_SALE] ([FOR_SALE_ID], [TYPE_NAME], [IS_ACTIVE], [LAST_CHANGED], [PRICE_PER_MONTH], [DETAILS]) VALUES (7, N'Site License', 0, CAST(0x00009FD600A9F406 AS DateTime), 100, NULL)
----SET IDENTITY_INSERT [dbo].[TB_FOR_SALE] OFF
----GO



--/****** Object:  StoredProcedure [dbo].[st_CheckGhostAccountBeforeActivation]    Script Date: 01/14/2015 16:00:18 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckGhostAccountBeforeActivation]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckGhostAccountBeforeActivation]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CheckGhostAccountBeforeActivation]    Script Date: 01/14/2015 16:00:18 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_CheckGhostAccountBeforeActivation] 
--(
--	@CONTACT_ID int,
--	--@USERNAME nvarchar(50),
--	@EMAIL nvarchar(500),
--	@RESULT nvarchar(100) out
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    Declare @Chk int = 0
--  --   set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where USERNAME = @USERNAME and CONTACT_ID != @CONTACT_ID)
--  --   if @Chk > 0
--  --   begin
--		--set @RESULT = 'Username is already in use, please choose a different username'
--		--RETURN
--  --   end
--  --   else
--  --   begin
--		set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where EMAIL = @EMAIL and CONTACT_ID != @CONTACT_ID)
--		if @Chk > 0 set @RESULT = 'E-Mail is already in use'
--		else
--		begin
--			select @Chk = COUNT(contact_id) from Reviewer.dbo.TB_CONTACT where [EXPIRY_DATE] is null and PWASHED is null and CONTACT_ID = @CONTACT_ID
--			if @Chk = 0
--			begin
--				select @RESULT = 'E-Mail is not in use, but Contact_ID does not match a Ghost account'
--			end
--			if @Chk = 1
--			begin
--				set @RESULT = 'Valid'
--				update Reviewer.dbo.TB_CONTACT set [EMAIL] = @EMAIL where CONTACT_ID = @CONTACT_ID
--			end
			
--		end

--END



--GO


--/****** Object:  StoredProcedure [dbo].[st_CheckLinkAddFailureLog]    Script Date: 01/14/2015 16:01:10 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckLinkAddFailureLog]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckLinkAddFailureLog]
--GO


--/****** Object:  StoredProcedure [dbo].[st_CheckLinkAddFailureLog]    Script Date: 01/14/2015 16:01:10 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_CheckLinkAddFailureLog]
--	-- Add the parameters for the stored procedure here
	
--	@CID int = null
--	, @UID uniqueidentifier = null
--	, @TYPE varchar(15) = null
--	, @QUERY_ST nvarchar(1000) = null
--	, @IP_B1 tinyint
--	, @IP_B2 tinyint
--	, @IP_B3 tinyint
--	, @IP_B4 tinyint
--	, @REASON nchar(500)
	
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- First add the line to the log
--	INSERT into TB_CHECKLINK_LOG
--           ([CHECKLINK_UID]
--           ,[TYPE]
--           ,[QUERY_STRING]
--           ,[CONTACT_ID]
--           ,[FAILURE_REASON]
--           ,[ALREADY_SENT]
--           ,[IP_B1]
--           ,[IP_B2]
--           ,[IP_B3]
--           ,[IP_B4])
--     VALUES
--           (@UID
--           ,@TYPE
--           ,@QUERY_ST
--           ,@CID
--           ,@REASON
--           ,0
--           ,@IP_B1 
--			, @IP_B2 
--			, @IP_B3 
--			, @IP_B4)
--	--Second, grab the data that should be sent to eppisupport@ioe.ac.uk, if any
--	--this is complex to avoid sending one message per error in case many attempts are failing in short time-spans
	
--	DECLARE @count int
--	Declare @MinutesFromLastSent int
--	select @MinutesFromLastSent = DATEDIFF(MINUTE,  max(DATE_CREATED), GETDATE()) from TB_CHECKLINK_LOG where ALREADY_SENT = 1
	
--	if @MinutesFromLastSent >= 30 --OK to send, last email was sent at least 30 minutes ago
--	BEGIN
--		UPDATE TB_CHECKLINK_LOG set ALREADY_SENT = 1 where ALREADY_SENT = 0
--		Select top(@@ROWCOUNT) * from TB_CHECKLINK_LOG order by DATE_CREATED desc --should return the rows that need to be sent
--		--code side will read the rows and compose the email notif
--	END
	

--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_CheckLinkCheck]    Script Date: 01/14/2015 16:01:33 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckLinkCheck]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckLinkCheck]
--GO



--/****** Object:  StoredProcedure [dbo].[st_CheckLinkCheck]    Script Date: 01/14/2015 16:01:33 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_CheckLinkCheck]
--	-- Add the parameters for the stored procedure here
--	 @CID int
--	, @UID uniqueidentifier 
--	, @RESULT varchar(15) OUTPUT
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    DECLARE @DateCheck Datetime2(1)
--    DECLARE @Type varchar(15)
--    Select @DateCheck = DATE_CREATED, @Type = [TYPE] 
--		from TB_CHECKLINK where CONTACT_ID = @CID and CHECKLINK_UID = @UID and IS_STALE = 0 and WAS_SUCCESS is null
--		--actually, checking IS_STALE = 0 and WAS_SUCCESS is null here makes the first failure quite unspecific...
--	if @DateCheck is null OR @@ROWCOUNT != 1 
--	begin
--		set @RESULT = 'Not found'
--		return
--	end
--	--possible types are: CheckEmail, ResetPw and ActivateGhost
--	IF @Type = 'CheckEmail'
--	Begin
--		if DATEDIFF(D, @DateCheck, GETDATE()) <= 7
--		begin
--			set @RESULT = @Type
--			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 1 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
--		end
--		else 
--		Begin
--			set @RESULT = 'ExpiredCkEmail'
--			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
--		end
--		return
--	END
--	if @Type = 'ResetPw'
--	Begin
--		if DATEDIFF(MINUTE, @DateCheck, GETDATE()) <= 10
--		begin
--			set @RESULT = @Type
--			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE() where CHECKLINK_UID = @UID and @CID = CONTACT_ID
--		end
--		else
--		begin
--			set @RESULT = 'ExpiredResetPw'
--			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
--		end
--		return
--	END
--	if @Type = 'ActivateGhost'
--	Begin
--		if DATEDIFF(D, @DateCheck, GETDATE()) <= 14
--		begin
--			set @RESULT = @Type
--		end
--		else 
--		begin
--			set @RESULT = 'ExpiredActGhost'
--			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
--		end
--		return
--	END
	
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_CheckLinkCreate]    Script Date: 01/14/2015 16:01:56 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckLinkCreate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckLinkCreate]
--GO

--/****** Object:  StoredProcedure [dbo].[st_CheckLinkCreate]    Script Date: 01/14/2015 16:01:56 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_CheckLinkCreate]
--	-- Add the parameters for the stored procedure here
--	@TYPE varchar(15)
--	, @CID int
--	, @CC_EMAIL nvarchar(500) = null
--	, @UID uniqueidentifier OUTPUT
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    --should we check if the Contact_ID actually exist?
--	UPDATE TB_CHECKLINK set IS_STALE = 1 where [TYPE] = @TYPE and CONTACT_ID = @CID
	
--	SET @UID = NEWID()
--	INSERT INTO TB_CHECKLINK
--           (CHECKLINK_UID
--           ,TYPE
--           ,DATE_CREATED
--           ,CONTACT_ID
--           ,IS_STALE
--           ,DATE_USED
--           ,WAS_SUCCESS
--           ,CC_EMAIL
--           )
--     VALUES
--           (@UID
--           ,@TYPE
--           ,GETDATE()
--           ,@CID
--           ,0
--           ,null
--           ,null
--           ,@CC_EMAIL
--           )

--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactActivate]    Script Date: 01/14/2015 16:02:19 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactActivate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactActivate]
--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactActivate]    Script Date: 01/14/2015 16:02:19 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE procedure [dbo].[st_ContactActivate]
--(
--	@CONTACT_ID int
--)

--As

--SET NOCOUNT ON

--	declare @NUMBER_MONTHS int
--	declare @EXPIRY_DATE date
--	declare @creator_id int
--	declare @reason nvarchar(35) = 'The CreatorID activated the account'

--	select @NUMBER_MONTHS = MONTHS_CREDIT 
--	from Reviewer.dbo.TB_CONTACT
--	where CONTACT_ID = @CONTACT_ID and [EXPIRY_DATE] is null
	
--	--activating an account, this could be a ghost account (MONTHS_CREDIT != 0) or a normal one (MONTHS_CREDIT is null)
--	if @NUMBER_MONTHS is null OR @NUMBER_MONTHS = 0 
--	BEGIN
--		select @NUMBER_MONTHS = 1
--		select @reason = 'Email Verified'
--	end
	
--	set @EXPIRY_DATE = dateadd(month,@NUMBER_MONTHS, sysdatetime())
	
--	update Reviewer.dbo.TB_CONTACT
--	set [EXPIRY_DATE] = @EXPIRY_DATE, MONTHS_CREDIT = 0
--	where CONTACT_ID = @CONTACT_ID
	
--	select @creator_id = CREATOR_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID

--	-- add a line to TB_EXPIRY_EDIT_LOG to say the account has been activated
--	insert into ReviewerAdmin.dbo.TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE,
--		NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--	values (GETDATE(), 1, @CONTACT_ID, null, DATEADD(month, @NUMBER_MONTHS, GETDATE()), 
--		@creator_id, 19, @reason)
	
	

--SET NOCOUNT OFF

--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactCheckUnameOrEmail]    Script Date: 01/14/2015 16:02:38 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactCheckUnameOrEmail]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactCheckUnameOrEmail]
--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactCheckUnameOrEmail]    Script Date: 01/14/2015 16:02:38 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactCheckUnameOrEmail]
--	-- Add the parameters for the stored procedure here
--	@Uname varchar(50) = '' OUTPUT
--	,@Email nvarchar(500) = '' OUTPUT
--	,@CID int = 0 OUTPUT
--	,@CONTACT_NAME nvarchar(255) = '' OUTPUT
--	,@IS_ACTIVE bit = 0 OUTPUT
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	if (@Uname is not null and @Uname != '') AND (@Email is null or @Email = '')
--	begin 
--		Select @CID = CONTACT_ID, @Email = EMAIL, @CONTACT_NAME = CONTACT_NAME, 
--					@IS_ACTIVE = CASE 
--							WHEN FLAVOUR IS NULL OR EXPIRY_DATE is null then 0
--							ELSE 1
--						END
--		  from Reviewer.dbo.TB_CONTACT where @Uname = USERNAME
--	End
--	ELSE
--	begin 
--		if (@Email is not null and @Email != '') AND (@Uname is null or @Uname = '')
--		BEGIN
--			Select @CID = CONTACT_ID, @CONTACT_NAME = CONTACT_NAME, @Uname = USERNAME, 
--					@IS_ACTIVE = CASE 
--							WHEN FLAVOUR IS NULL OR EXPIRY_DATE is null then 0
--							ELSE 1
--						END
--		   from Reviewer.dbo.TB_CONTACT where @Email = EMAIL
--		END
--		ELSE 
--		BEGIN 
--			IF (@Email is not null and @Email != '') AND (@Uname is not null and @Uname != '')
--			BEGIN
--				Select @CID = CONTACT_ID, @CONTACT_NAME = CONTACT_NAME, 
--						@IS_ACTIVE = CASE 
--							WHEN FLAVOUR IS NULL OR EXPIRY_DATE is null then 0
--							ELSE 1
--						END
--		   from Reviewer.dbo.TB_CONTACT where @Email = EMAIL and @Uname = USERNAME
--			END
--		END
--	END
--	if @CID is null select @CID = 0
--	if @CONTACT_NAME is null select @CONTACT_NAME = ''
--	if @Uname is null select @Uname = ''
--	if @IS_ACTIVE is null select @IS_ACTIVE = 1 --we don't want to do stuff when we didn't find the account that needs activating
--END

--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactCreate]    Script Date: 01/14/2015 16:03:09 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactCreate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactCreate]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactCreate]    Script Date: 01/14/2015 16:03:09 ******/
--SET ANSI_NULLS OFF
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE PROCEDURE [dbo].[st_ContactCreate]
--(
--	@CONTACT_NAME nvarchar(255), 
--	@USERNAME nvarchar(50), 
--	@PASSWORD varchar(50), 
--	@DATE_CREATED datetime, 
--	--@EXPIRY_DATE date, 
--	@EMAIL nvarchar(500),
--	@DESCRIPTION nvarchar(1000),
--	@CONTACT_ID int output
--)
--AS

--	declare @NEW_CONTACT_ID int
--	--create salt!
--	DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
--	declare @rnd varchar(20)
--	declare @cnt int = 0
--	set @rnd = ''
--	WHILE (@cnt <= 20) 
--	BEGIN
--		SELECT @rnd = @rnd + 
--			SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
--		SELECT @cnt = @cnt + 1
--	END
		
--	INSERT INTO Reviewer.dbo.TB_CONTACT(CONTACT_NAME, USERNAME, DATE_CREATED, [EXPIRY_DATE], EMAIL, DESCRIPTION
--										,FLAVOUR, PWASHED)
--	VALUES (@CONTACT_NAME, @USERNAME, @DATE_CREATED, Null, @EMAIL, @DESCRIPTION
--			, @rnd, HASHBYTES('SHA1', @PASSWORD + @rnd))
	
--	set @NEW_CONTACT_ID = @@IDENTITY
	
--	update Reviewer.dbo.TB_CONTACT set CREATOR_ID = @NEW_CONTACT_ID
--	where CONTACT_ID = @NEW_CONTACT_ID
	
--	select @CONTACT_ID = CONTACT_ID from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @NEW_CONTACT_ID
	
--	RETURN

--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 01/14/2015 16:03:34 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactDetails]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactDetails]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactDetails]    Script Date: 01/14/2015 16:03:34 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactDetails] 
--(
--	@CONTACT_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--    max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--				CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE', 
--	c.MONTHS_CREDIT, c.CREATOR_ID,
--    c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], c.SEND_NEWSLETTER,
--    l.SITE_LIC_ID, l.SITE_LIC_NAME
--    ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--	from Reviewer.dbo.TB_CONTACT c
--	left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--	on c.CONTACT_ID = lt.CONTACT_ID
--	left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--	left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--	where c.CONTACT_ID = @CONTACT_ID
	
--	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--	c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID, c.SEND_NEWSLETTER,
--    c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

--	RETURN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--END
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactEdit]    Script Date: 01/14/2015 16:03:58 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactEdit]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactEdit]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactEdit]    Script Date: 01/14/2015 16:03:58 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE procedure [dbo].[st_ContactEdit]
--(
--	@CONTACT_NAME nvarchar(255),
--	@USERNAME nvarchar(50),
--	@EMAIL nvarchar(500),
--	@PASSWORD varchar(50),
--	@CONTACT_ID int
--)

--As

--SET NOCOUNT ON

--	if @PASSWORD = ''
--	begin
--		update Reviewer.dbo.TB_CONTACT 
--		set CONTACT_NAME = @CONTACT_NAME,
--		USERNAME = @USERNAME,
--		EMAIL = @EMAIL
--		where CONTACT_ID = @CONTACT_ID
--    end
--	else
--	begin
--		--create salt!
--		DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
--		declare @rnd varchar(20)
--		declare @cnt int = 0
--		set @rnd = ''
--		WHILE (@cnt <= 20) 
--		BEGIN
--			SELECT @rnd = @rnd + 
--				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
--			SELECT @cnt = @cnt + 1
--		END
--		update Reviewer.dbo.TB_CONTACT 
--		set CONTACT_NAME = @CONTACT_NAME
--			, USERNAME = @USERNAME
--			, EMAIL = @EMAIL
--			, FLAVOUR = @rnd
--			, PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
--		where CONTACT_ID = @CONTACT_ID
--	end

--SET NOCOUNT OFF

--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 01/14/2015 16:04:23 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactLogin]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactLogin]
--GO


--/****** Object:  StoredProcedure [dbo].[st_ContactLogin]    Script Date: 01/14/2015 16:04:23 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Jeff>
---- ALTER date: <24/03/10>
---- Description:	<gets contact details when loging in>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactLogin] 
--(
--	@USERNAME varchar(50),
--	@PASSWORD varchar(50),
--	@IP_ADDRESS nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;
----first check if the username/pw are correct

--	SELECT c.*, COUNT(sla.CONTACT_ID) as IsSLA
--	FROM Reviewer.dbo.TB_CONTACT c
--	Left outer join TB_SITE_LIC_ADMIN sla on sla.CONTACT_ID = c.CONTACT_ID
--	where c.USERNAME = @USERNAME
--	and HASHBYTES('SHA1', @Password + FLAVOUR) = PWASHED
--	and EXPIRY_DATE is not null
--	group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME,
--	c.PASSWORD, c.LAST_LOGIN, c.DATE_CREATED, c.EMAIL, c.EXPIRY_DATE,
--	c.MONTHS_CREDIT, c.CREATOR_ID,c.TYPE, c.IS_SITE_ADMIN, c.DESCRIPTION, c.SEND_NEWSLETTER, c.FLAVOUR, c.PWASHED
	
--	RETURN
--END


--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 01/14/2015 16:06:54 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactPurchasedDetails]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactPurchasedDetails]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ContactPurchasedDetails]    Script Date: 01/14/2015 16:06:54 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:		<Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactPurchasedDetails] 
--(
--	@CONTACT_ID nvarchar(50)
--)
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--        declare @PURCHASED_ACCOUNTS table ( 
--	CONTACT_ID int, 
--	CONTACT_NAME nchar(1000),
--	EMAIL nvarchar(500),
--	DATE_CREATED datetime,
--	[EXPIRY_DATE] date,
--	MONTHS_CREDIT smallint,
--	CREATOR_ID int, 
--	LAST_LOGIN datetime,
--	SITE_LIC_ID int null,
--	SITE_LIC_NAME nvarchar(50) null,
--	FLAVOUR char(20) null,
--	IS_FULLY_ACTIVE bit null,
--	IS_STALE_AGHOST bit null)

--	Insert Into @PURCHASED_ACCOUNTS (CONTACT_ID, CONTACT_NAME, EMAIL, DATE_CREATED, [EXPIRY_DATE],
--					MONTHS_CREDIT, CREATOR_ID, SITE_LIC_ID, SITE_LIC_NAME, FLAVOUR, IS_FULLY_ACTIVE, IS_STALE_AGHOST)
--	SELECT c.[CONTACT_ID], [CONTACT_NAME], EMAIL, c.[DATE_CREATED],
--		 CASE when l.[EXPIRY_DATE] is not null 
--				and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--					then l.[EXPIRY_DATE]
--				else c.[EXPIRY_DATE]
--				end as 'EXPIRY_DATE',
--		  [MONTHS_CREDIT], c.[CREATOR_ID], l.SITE_LIC_ID, l.SITE_LIC_NAME
--		  ,FLAVOUR, 0, null
--	  FROM [Reviewer].[dbo].[TB_CONTACT] c 
--	  left outer join Reviewer.dbo.TB_SITE_LIC_CONTACT lc on c.CONTACT_ID = lc.CONTACT_ID
--	  left outer join Reviewer.dbo.TB_SITE_LIC l on lc.SITE_LIC_ID = l.SITE_LIC_ID
--	  where c.CREATOR_ID = @CONTACT_ID
--	  and ((c.EXPIRY_DATE is null and MONTHS_CREDIT != 0)
--			or
--			(c.EXPIRY_DATE is not null))
--	and c.CONTACT_ID != @CONTACT_ID

--	update  p_a --add usage info
--	set p_a.LAST_LOGIN = l_t.CREATED
--	--max(l_t.CREATED)
--	from @PURCHASED_ACCOUNTS p_a, TB_LOGON_TICKET l_t
--	where l_t.CREATED in
--	(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
--	where p_a.CONTACT_ID = l_t.CONTACT_ID)
	
--	--set whether the account is fully active (the person who purchased it won't be able to edit the account details)
--	update t set IS_FULLY_ACTIVE = 1
--	from @PURCHASED_ACCOUNTS t where EMAIL is not null 
--			and CONTACT_NAME is not null and [EXPIRY_DATE] is not null and FLAVOUR is not null
	
--	--we need to figure out if ghost accounts awaiting for activation are out of time and need re-activation
--	declare @DATES table (CONTACT_ID int, MDATE Datetime2(1))
			
--	insert into @DATES
--		SELECT t.CONTACT_ID,  MAX(c.DATE_CREATED)
--	from @PURCHASED_ACCOUNTS t inner join TB_CHECKLINK c on t.CONTACT_ID = c.CONTACT_ID
--											and c.TYPE = 'ActivateGhost'
--											and t.IS_FULLY_ACTIVE = 0
--		group by t.CONTACT_ID, c.TYPE, t.IS_FULLY_ACTIVE
--	update t set IS_STALE_AGHOST = 1
--	from @PURCHASED_ACCOUNTS t inner join @DATES d on t.CONTACT_ID = d.CONTACT_ID and DATEDIFF(D, d.MDATE, GETDATE()) > 14
	
--	--mark ghost account that are waiting for the end user to activate them but are still in time to do so
--	update t set IS_STALE_AGHOST = 0
--	from @PURCHASED_ACCOUNTS t where t.CONTACT_NAME is null and t.FLAVOUR is null and IS_STALE_AGHOST is null and EMAIL is not null
--	--email field is populated to send the activation request to the intended user
	
--	select CONTACT_ID , 
--	CONTACT_NAME ,
--	EMAIL ,
--	DATE_CREATED ,
--	[EXPIRY_DATE] ,
--	MONTHS_CREDIT ,
--	CREATOR_ID , 
--	LAST_LOGIN ,
--	SITE_LIC_ID ,
--	SITE_LIC_NAME  ,
--	IS_FULLY_ACTIVE ,
--	IS_STALE_AGHOST 
--	 from @PURCHASED_ACCOUNTS
    

--	RETURN

--END

--GO
--/****** Object:  StoredProcedure [dbo].[st_EmailUpdate]    Script Date: 01/14/2015 16:07:21 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_EmailUpdate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_EmailUpdate]
--GO
--/****** Object:  StoredProcedure [dbo].[st_EmailUpdate]    Script Date: 01/14/2015 16:07:21 ******/
--SET ANSI_NULLS OFF
--GO

--SET QUOTED_IDENTIFIER ON
--GO



--CREATE PROCEDURE [dbo].[st_EmailUpdate]
--(
--	@EMAIL_ID int,
--	@EMAIL_NAME nvarchar(50),
--	@EMAIL_MESSAGE nvarchar(4000)
--)
--As
--SET NOCOUNT ON


--		update TB_MANAGMENT_EMAILS
--		set EMAIL_MESSAGE = @EMAIL_MESSAGE, EMAIL_NAME = @EMAIL_NAME
--		where EMAIL_ID = @EMAIL_ID 


--SET NOCOUNT OFF
--GO

--/****** Object:  StoredProcedure [dbo].[st_GhostContactActivate]    Script Date: 01/14/2015 16:07:47 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GhostContactActivate]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_GhostContactActivate]
--GO

--/****** Object:  StoredProcedure [dbo].[st_GhostContactActivate]    Script Date: 01/14/2015 16:07:47 ******/
--SET ANSI_NULLS OFF
--GO

--SET QUOTED_IDENTIFIER ON
--GO

--CREATE PROCEDURE [dbo].[st_GhostContactActivate]
--(
--	@CONTACT_NAME nvarchar(255), 
--	@USERNAME nvarchar(50), 
--	@PASSWORD varchar(50), 
--	@EMAIL nvarchar(500),
--	@DESCRIPTION nvarchar(1000),
--	@CONTACT_ID int,
--	@UID uniqueidentifier,
--	@RES nvarchar(50) = 'Done' output
--)
--AS
--	declare @chk int = 0
--	select @RES = 'Done'
--	select @chk = COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID
--	if @chk = 0
--	begin
--		select @RES = 'User does not exist'
--		return
--	end
	
--	select @chk = COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID 
--					and (EXPIRY_DATE is null OR PWASHED is null)
--	if @chk != 1
--	begin
--		select @RES = 'Not a ghost account'
--		return
--	end
	
--	--create salt!
--	DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
--	declare @rnd varchar(20)
--	declare @cnt int = 0
--	set @rnd = ''
--	WHILE (@cnt <= 20) 
--	BEGIN
--		SELECT @rnd = @rnd + 
--			SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
--		SELECT @cnt = @cnt + 1
--	END
--	update Reviewer.dbo.TB_CONTACT set 
--		[EMAIL] = @EMAIL 
--		,CONTACT_NAME = @CONTACT_NAME
--		,USERNAME = @USERNAME
--		,[DESCRIPTION] = @DESCRIPTION
--		,PWASHED = HASHBYTES('SHA1', @PASSWORD + @rnd)
--		, FLAVOUR = @rnd
--	where CONTACT_ID = @CONTACT_ID	
	
--	EXEC dbo.st_ContactActivate	@CONTACT_ID --activate the account by setting the expiry date
	
--	--check if this worked
--	Select @chk = COUNT(contact_id) from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID and EXPIRY_DATE is not null
--	if @chk != 1
--	BEGIN --not good
--		select @RES = 'Tried to activate, but something didn''t work'
--		update TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 0, DATE_USED = GETDATE() where CONTACT_ID = @CONTACT_ID and CHECKLINK_UID = @UID
--	END
--	ELSE
--	Begin
--		update TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 1, DATE_USED = GETDATE() where CONTACT_ID = @CONTACT_ID and CHECKLINK_UID = @UID
--	END
	
--	RETURN

--GO

--/****** Object:  StoredProcedure [dbo].[st_GhostContactActivateRevoke]    Script Date: 01/14/2015 16:08:06 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_GhostContactActivateRevoke]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_GhostContactActivateRevoke]
--GO

--/****** Object:  StoredProcedure [dbo].[st_GhostContactActivateRevoke]    Script Date: 01/14/2015 16:08:06 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_GhostContactActivateRevoke]
--	-- Add the parameters for the stored procedure here
--	@CONTACT_ID int
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	Update TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE() where 
--		IS_STALE != 1 and CONTACT_ID = @CONTACT_ID and [TYPE] = 'ActivateGhost'
--	if @@ROWCOUNT > 0
--	begin
--		Update Reviewer.dbo.TB_CONTACT set EMAIL = null where CONTACT_ID = @CONTACT_ID and FLAVOUR is null and EXPIRY_DATE is null
--	end
--END

--GO

--/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Ticket]    Script Date: 01/14/2015 16:08:39 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_LogonTicket_Check_Ticket]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_LogonTicket_Check_Ticket]
--GO

--/****** Object:  StoredProcedure [dbo].[st_LogonTicket_Check_Ticket]    Script Date: 01/14/2015 16:08:39 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_LogonTicket_Check_Ticket]
--	-- Add the parameters for the stored procedure here
--	@guid uniqueidentifier, 
--	@c_ID int,
--	@RID int OUTPUT
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	select @RID = 0
--	select @RID = l.REVIEW_ID from TB_LOGON_TICKET l where l.TICKET_GUID = @guid and l.STATE = 1 and l.CONTACT_ID = @c_ID
--	if @RID is null select @RID = 0
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_ResetPassword]    Script Date: 01/14/2015 16:10:46 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ResetPassword]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ResetPassword]
--GO

--/****** Object:  StoredProcedure [dbo].[st_ResetPassword]    Script Date: 01/14/2015 16:10:46 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ResetPassword] 
--	-- Add the parameters for the stored procedure here
--	@CID int
--	, @UID uniqueidentifier
--	, @UNAME varchar(50)
--	, @PW varchar(50)
--	, @RES int = 0 OUTPUT
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	SELECT @RES = COUNT(c.CONTACT_ID) from Reviewer.dbo.TB_CONTACT c
--		inner join TB_CHECKLINK l on c.CONTACT_ID = l.CONTACT_ID and c.CONTACT_ID = @CID
--		where l.CHECKLINK_UID = @UID and c.USERNAME = @UNAME and l.IS_STALE = 1 
--			and l.WAS_SUCCESS is null --should we not check for this and allow the user to correct the username in case it was mispelled?
--			and (@pw is not null and LEN(@pw) > 7)
--	IF @RES = 1 --all is well: we found one result as expected
--	BEGIN
--		--PW hash!
--		DECLARE @chars char(100) = '!ò#$%&à()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~èéêëì'
--		declare @rnd varchar(20)
--		declare @cnt int = 0
--		set @rnd = ''
--		WHILE (@cnt <= 20) 
--		BEGIN
--			SELECT @rnd = @rnd + 
--				SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
--			SELECT @cnt = @cnt + 1
--		END
--		update c
--		set c.FLAVOUR = @rnd, c.PWASHED = HASHBYTES('SHA1', @pw + @rnd)
--		from Reviewer.dbo.TB_CONTACT c
--			inner join TB_CHECKLINK l on c.CONTACT_ID = l.CONTACT_ID and c.CONTACT_ID = @CID
--		where l.CHECKLINK_UID = @UID and c.USERNAME = @UNAME and l.IS_STALE = 1 and l.WAS_SUCCESS is null
--		Select @RES = @@ROWCOUNT
--		if @RES = 1
--		begin --still doing good: we changed one row as expected
--			--mark the link as used successfully 
--			UPDATE TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 1, DATE_USED = GETDATE() where CONTACT_ID = @CID and CHECKLINK_UID = @UID
--		end
--		ELSE BEGIN
--			UPDATE TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 0, DATE_USED = GETDATE() where CONTACT_ID = @CID and CHECKLINK_UID = @UID
--			select @RES = -1 --means the failure happened when trying to change the password
--		END
--	END
--	ELSE BEGIN
--		UPDATE TB_CHECKLINK set IS_STALE = 1, WAS_SUCCESS = 0, DATE_USED = GETDATE() where CONTACT_ID = @CID and CHECKLINK_UID = @UID
--		select @RES = 0 --means the failure happened when trying to validate the link/account info
--	END
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_TransferAccountCredit]    Script Date: 01/14/2015 16:11:13 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_TransferAccountCredit]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_TransferAccountCredit]
--GO



--/****** Object:  StoredProcedure [dbo].[st_TransferAccountCredit]    Script Date: 01/14/2015 16:11:13 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO

---- =============================================
---- Author:		<Author,,Name>
---- Create date: <Create Date,,>
---- Description:	<Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_TransferAccountCredit] 
--	-- Add the parameters for the stored procedure here
--	@CONTACT_ID int,
--	@PURCHASER_ID int,
--	@EMAIL nvarchar(500),
--	@RESULT nvarchar(100) out,
--	@CREDIT smallint out,
--	@NEWDATE date out,
--	@CONTACT_NAME nvarchar(255) out
--AS
--BEGIN
--	-- SET NOCOUNT ON added to prevent extra result sets from
--	-- interfering with SELECT statements.
--	SET NOCOUNT ON;

--    -- Insert statements for procedure here
--	declare @DEST_CID int = 0
--	select @DEST_CID = contact_id from Reviewer.dbo.TB_CONTACT c where @CONTACT_ID != c.CONTACT_ID and @EMAIL = c.EMAIL
	
--	IF (@DEST_CID is null OR @DEST_CID < 1)--account was not found
--	BEGIN
--		select @RESULT = 'The destination account was not found'
--		return
--	END
	
--	select @CREDIT = c.MONTHS_CREDIT from Reviewer.dbo.TB_CONTACT c where @CONTACT_ID = c.CONTACT_ID and (@EMAIL != c.EMAIL OR c.EMAIL is null)
--	if (@CREDIT is null OR @CREDIT < 1)--there is no credit to transfer
--	BEGIN
--		select @RESULT = 'There is no credit to transfer'
--		return
--	END
--	--checks worked, so we know what to do: add to
--	if (@CREDIT > 1) select @CREDIT = @CREDIT - 1 --ghost accounts get the purchased amount + 1 month free trial, we need to remove this
--	declare @oldDate DATE
--	select @oldDate = c.[EXPIRY_DATE] from  Reviewer.dbo.TB_CONTACT c where c.CONTACT_ID = @DEST_CID
--	UPDATE c set c.EXPIRY_DATE = CASE 
--									when (c.[EXPIRY_DATE] is null) then null
--									when (c.[EXPIRY_DATE] > getdate()) then DATEADD(month, @CREDIT, c.[EXPIRY_DATE])
--									ELSE DATEADD(month, @CREDIT, getdate())
--								  END
--			     ,c.MONTHS_CREDIT = CASE 
--									WHEN c.EXPIRY_DATE is null AND c.MONTHS_CREDIT is not null
--										THEN c.MONTHS_CREDIT + @CREDIT
--									WHEN c.EXPIRY_DATE is null AND c.MONTHS_CREDIT is null
--										THEN @CREDIT + 1 --this is a new account that was never activated
--									ELSE 0
--								  END
--		from Reviewer.dbo.TB_CONTACT c where c.CONTACT_ID = @DEST_CID
--	declare @chk int
--	select @chk = COUNT(CONTACT_ID) from Reviewer.dbo.TB_CONTACT where @DEST_CID = CONTACT_ID 
--										and (
--											 ([EXPIRY_DATE] is not null AND [EXPIRY_DATE] > GETDATE())
--											  OR 
--											  (MONTHS_CREDIT is not null AND MONTHS_CREDIT >= @CREDIT)
--											)
--	if @chk = 1
--		BEGIN --all good! remove previous account, update bill info (to show the dest account in the old bill)
--		-- update the extensions log and return
--		declare @BillLine int
--		select @BillLine = MAX(bl.LINE_ID) from TB_BILL_LINE bl
--			inner join TB_BILL b on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'OK: Paid and data committed'
--					AND AFFECTED_ID = @CONTACT_ID
--		update TB_BILL_LINE set AFFECTED_ID = @DEST_CID where LINE_ID = @BillLine
		
--		DELETE from Reviewer.dbo.TB_CONTACT where CONTACT_ID = @CONTACT_ID --ghost is now empty, why keep it?
		
--		--mark the extensios as a "Purchase"
--		 insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
--			OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
--		 select GETDATE(), 1, @DEST_CID, [EXPIRY_DATE],
--			@oldDate, @PURCHASER_ID, 2, 'Transfer ghost credit, from ID =' + CONVERT(varchar(10), @CONTACT_ID) + ' (deleted).'
--		 from Reviewer.dbo.TB_CONTACT where @DEST_CID = CONTACT_ID 
		 
--		 --get the affected account details
--		 Select @RESULT = 'Success', @NEWDATE = c.EXPIRY_DATE , @CONTACT_NAME = c.CONTACT_NAME
--			from Reviewer.dbo.TB_CONTACT c where @DEST_CID = CONTACT_ID 
--		 return
--	END	
--	select @RESULT = 'Unspecified error: checks where successful but credit was not transferred'
--END

--GO


--/****** Object:  StoredProcedure [dbo].[st_CodesetsGet]    Script Date: 01/27/2015 16:27:31 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CodesetsGet]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CodesetsGet]
--GO



--/****** Object:  StoredProcedure [dbo].[st_CodesetsGet]    Script Date: 01/27/2015 16:27:31 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



--CREATE procedure [dbo].[st_CodesetsGet]
--(
--                @REVIEW_ID int
--)

--As

--SET NOCOUNT ON

--                select s.SET_ID, s.SET_NAME, st.SET_TYPE from Reviewer.dbo.TB_SET s 
--                inner join Reviewer.dbo.TB_REVIEW_SET rs
--                on s.SET_ID = rs.SET_ID
--                inner join Reviewer.dbo.TB_SET_TYPE st
--                on s.SET_TYPE_ID = st.SET_TYPE_ID
--                and REVIEW_ID = @REVIEW_ID

--SET NOCOUNT OFF


--GO

--/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsFilter]    Script Date: 01/27/2015 16:30:00 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ReviewDetailsFilter]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ReviewDetailsFilter]
--GO



--/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsFilter]    Script Date: 01/27/2015 16:30:00 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO


---- =============================================
---- Author:                            <Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:   <Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ReviewDetailsFilter] 
--(
--                @SHAREABLE bit,
--                @TEXT_BOX nvarchar(255)
--)
--AS
--BEGIN
--                -- SET NOCOUNT ON added to prevent extra result sets from
--                -- interfering with SELECT statements.
--                SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    if @SHAREABLE = 1
--                begin        
--                                /*
--                                SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
--                                r.FUNDER_ID, c.CONTACT_NAME, r.MONTHS_CREDIT
--                                FROM Reviewer.dbo.tb_REVIEW r
--                                inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
--                                where 
--                                (r.EXPIRY_DATE is not null) OR
--                                (r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0)*/
                                
--                                select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME,  
--                                r.DATE_CREATED, 
--                                                CASE when l.[EXPIRY_DATE] is not null 
--                                                and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
--                                                                then l.[EXPIRY_DATE]
--                                                else r.[EXPIRY_DATE]
--                                                end as 'EXPIRY_DATE', 
--                                r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
--                                l.SITE_LIC_ID, l.SITE_LIC_NAME
--                                from Reviewer.dbo.TB_REVIEW r
--                                inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
--                                left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
--                                left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
--                                where 
--                                                ((r.EXPIRY_DATE is not null) OR
--                                                (r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0))
--                                and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
--                                                (r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
--                                                (c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
                                
                                
--                                group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
--                                r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
--                                l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
--                                order by r.REVIEW_NAME
                                
--                end
--                else
--                begin
--                                SELECT r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED, r.EXPIRY_DATE, 
--                                r.FUNDER_ID, c.CONTACT_NAME
--                                FROM Reviewer.dbo.tb_REVIEW r
--                                inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID 
--                                where (r.EXPIRY_DATE is null and r.MONTHS_CREDIT = 0)
                                
--                                and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
--                                                (r.REVIEW_NAME like '%' + @TEXT_BOX + '%') OR
--                                                (c.CONTACT_NAME like '%' + @TEXT_BOX + '%'))
                                
--                end

--                RETURN

--END
--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAllFilter]    Script Date: 01/27/2015 16:31:07 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactDetailsGetAllFilter]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactDetailsGetAllFilter]
--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactDetailsGetAllFilter]    Script Date: 01/27/2015 16:31:07 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO




---- =============================================
---- Author:                            <Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:   <Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactDetailsGetAllFilter] 
--(
--                @ER4AccountsOnly bit,
--                @TEXT_BOX nvarchar(255)
--)
--AS
--BEGIN
--                -- SET NOCOUNT ON added to prevent extra result sets from
--                -- interfering with SELECT statements.
--                SET NOCOUNT ON;

--                if @ER4AccountsOnly = 1
--                begin        
--                                select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--                                max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--                                                                CASE when l.[EXPIRY_DATE] is not null 
--                                                                and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--                                                                                then l.[EXPIRY_DATE]
--                                                                else c.[EXPIRY_DATE]
--                                                                end as 'EXPIRY_DATE', 
--                                c.MONTHS_CREDIT, c.CREATOR_ID,
--                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
--                                l.SITE_LIC_ID, l.SITE_LIC_NAME
--                                ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--                                from Reviewer.dbo.TB_CONTACT c
--                                left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--                                on c.CONTACT_ID = lt.CONTACT_ID
--                                left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--                                left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--                                where (c.EXPIRY_DATE > '2010-03-20 00:00:01' or
--                                (c.EXPIRY_DATE is null and MONTHS_CREDIT != 0))
                                
--                                and ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
--                                                (c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
--                                                (c.EMAIL like '%' + @TEXT_BOX + '%'))
                                
--                                group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--                                c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
--                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
                                
--                end
--                else
--                begin
--                                select c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], c.EMAIL, 
--                                max(lt.CREATED) as LAST_LOGIN, c.DATE_CREATED, 
--                                                                CASE when l.[EXPIRY_DATE] is not null 
--                                                                and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
--                                                                                then l.[EXPIRY_DATE]
--                                                                else c.[EXPIRY_DATE]
--                                                                end as 'EXPIRY_DATE', 
--                                c.MONTHS_CREDIT, c.CREATOR_ID,
--                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION],
--                                l.SITE_LIC_ID, l.SITE_LIC_NAME
--                                ,SUM(DATEDIFF(HOUR,lt.CREATED ,lt.LAST_RENEWED )) as [active_hours]
--                                from Reviewer.dbo.TB_CONTACT c
--                                left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
--                                on c.CONTACT_ID = lt.CONTACT_ID
--                                left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
--                                left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
--                                where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
--                                                                (c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
--                                                                (c.EMAIL like '%' + @TEXT_BOX + '%'))
                                
--                                group by c.CONTACT_ID, c.old_contact_id, c.CONTACT_NAME, c.USERNAME, c.[PASSWORD], 
--                                c.DATE_CREATED, c.[EXPIRY_DATE], c.EMAIL, c.MONTHS_CREDIT, c.CREATOR_ID,
--                                c.MONTHS_CREDIT, c.[TYPE], c.IS_SITE_ADMIN, c.[DESCRIPTION], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME
                                
--                end
       
--END




--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactReviewsFilter]    Script Date: 01/27/2015 16:37:33 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_ContactReviewsFilter]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_ContactReviewsFilter]
--GO



--/****** Object:  StoredProcedure [dbo].[st_ContactReviewsFilter]    Script Date: 01/27/2015 16:37:33 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



---- =============================================
---- Author:                            <Jeff>
---- Create date: <24/03/2010>
---- Description:   <gets the review data based on contact>
---- =============================================
--CREATE PROCEDURE [dbo].[st_ContactReviewsFilter] 
--(
--                @CONTACT_ID nvarchar(50),
--                @TEXT_BOX nvarchar(255)
--)
--AS
--BEGIN
--                -- SET NOCOUNT ON added to prevent extra result sets from
--                -- interfering with SELECT statements.
--                SET NOCOUNT ON;
                
--    -- Insert statements for procedure here
--    select r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED,
--                SUM(DATEDIFF(HOUR,t.CREATED ,t.LAST_RENEWED )) as HOURS 
--                from Reviewer.dbo.TB_REVIEW r
--                inner join Reviewer.dbo.TB_REVIEW_CONTACT r_c on r_c.REVIEW_ID = r.REVIEW_ID
--                inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r_c.CONTACT_ID
--                left outer join ReviewerAdmin.dbo.[TB_LOGON_TICKET] t on t.REVIEW_ID = r_c.REVIEW_ID and t.CONTACT_ID = r_c.CONTACT_ID
--                where r_c.CONTACT_ID = @CONTACT_ID
                
--                and ((r.REVIEW_ID like '%' + @TEXT_BOX + '%') OR
--                                                                (r.REVIEW_NAME like '%' + @TEXT_BOX + '%'))
                
--                group by r.REVIEW_ID, r.REVIEW_NAME, r.DATE_CREATED


--END



--GO


--/****** Object:  StoredProcedure [dbo].[st_CheckUserNameAndEmail]    Script Date: 01/27/2015 16:38:34 ******/
--IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CheckUserNameAndEmail]') AND type in (N'P', N'PC'))
--DROP PROCEDURE [dbo].[st_CheckUserNameAndEmail]
--GO


--/****** Object:  StoredProcedure [dbo].[st_CheckUserNameAndEmail]    Script Date: 01/27/2015 16:38:34 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO





---- =============================================
---- Author:                            <Author,,Name>
---- ALTER date: <ALTER Date,,>
---- Description:   <Description,,>
---- =============================================
--CREATE PROCEDURE [dbo].[st_CheckUserNameAndEmail] 
--(
--                @CONTACT_ID int,
--                @USERNAME nvarchar(50),
--                @EMAIL nvarchar(500),
--                @RESULT nvarchar(100) out
--)
--AS
--BEGIN
--                -- SET NOCOUNT ON added to prevent extra result sets from
--                -- interfering with SELECT statements.
--                SET NOCOUNT ON;

--    -- Insert statements for procedure here
--    Declare @Chk int = 0
--     set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where USERNAME = @USERNAME and CONTACT_ID != @CONTACT_ID)
--     if @Chk > 0
--     begin
--                                set @RESULT = 'Username is already in use, please choose a different username'
--                                RETURN
--     end
--     else
--     begin
--                                set @Chk = (select COUNT(Contact_id) from Reviewer.dbo.TB_CONTACT where EMAIL = @EMAIL and CONTACT_ID != @CONTACT_ID)
--                                if @Chk > 0 set @RESULT = 'E-Mail is already in use, please choose a different E-Mail'
--                                else set @RESULT = 'Valid'
--     end
--                RETURN

--END

--GO











