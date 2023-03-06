--Script to populate an EMPTY tempTestReviewer database with minimum data.
--This is the LAST script to run, and Requires to already have a copy of tempTestReviewer (uses it to create users).
--INSTRUCTIONS: (15/06/2016)
--BEFORE YOU START: set passwords!
-- find the following text: "Declare @SiteAdmPw varchar(50)", set the two passwords thereabout.



USE [tempTestReviewerAdmin]
GO

Declare @SiteAdmPw varchar(50) = 'aa123'
Declare @NormalUserPw varchar(50) = '123'
Declare @now datetime = GetDate()

DECLARE	@return_value int,
		@CONTACT_ID int

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Default SuperUser',
		@USERNAME = N'SuperC',
		@PASSWORD = @SiteAdmPw,
		@DATE_CREATED = @now,
		@EMAIL = N'fakeEmal@ucl.ac.uk',
		@DESCRIPTION = N'Default SuperUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

--SELECT	@CONTACT_ID as N'@CONTACT_ID'

--SELECT	'Return Value' = @return_value
update tempTestReviewer.dbo.TB_CONTACT set IS_SITE_ADMIN = 1, EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Default NormalUser',
		@USERNAME = N'NormalC',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'fakeEmal2@ucl.ac.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Alice Fake',
		@USERNAME = N'alice',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'alice@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Bob Fake',
		@USERNAME = N'bob',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'bob@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Steve Fake',
		@USERNAME = N'steve',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'steve@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID




EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Jane Fake',
		@USERNAME = N'Jane',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'Jane@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID



EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'John Fake',
		@USERNAME = N'john',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'john@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID


EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Tracy Fake',
		@USERNAME = N'tracy',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'tracy@EPPIReviewer.org.uk',
		@DESCRIPTION = N'Default NormalUser',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID



EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'David Fake',
		@USERNAME = N'david',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'david@expired.org.uk',
		@DESCRIPTION = N'Default NormalUser)',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(yy, 1, GETDATE())
	where CONTACT_ID = @CONTACT_ID


SET @now = DATEADD(month, -1, @now)

EXEC	@return_value = [dbo].[st_ContactCreate]
		@CONTACT_NAME = N'Mary Fake',
		@USERNAME = N'mary',
		@PASSWORD = @NormalUserPw,
		@DATE_CREATED = @now,
		@EMAIL = N'mary@expired.org.uk',
		@DESCRIPTION = N'Default NormalUser (expired)',
		@CONTACT_ID = @CONTACT_ID OUTPUT

update tempTestReviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(day, -1, GETDATE())
	where CONTACT_ID = @CONTACT_ID

GO





--commented as IDs don't agree with at least one SProc
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('No extension', 'This No change to the extension date', '111', '0')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Purchase', 'Extension due to purchase', '111', '1')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Staff', 'Extension for EPPI staff', '111', '2')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Maintenance', 'Extension for network down time', '111', '3')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Making review non-shareable', 'Change review to non-shareable', '010', '4')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Has budget code', 'The project has a budget code without definate expiry date', '111', '5')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Restart trial', 'The user never used their trial access', '111', '6')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Return read-only access', 'Move the expiry date to less than two months in the past', '111', '7')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Working on EPPI-Centre project', 'The user is working on an EPPI-Centre project', '111', '8')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Activate review', 'The review has been activated', '111', '9')
--INSERT INTO [TB_EXTENSION_TYPES]  ([EXTENSION_TYPE] ,[DESCRIPTION] ,[APPLIES_TO] ,[ORDER]) VALUES ('Activate user account', 'The user account has been activated', '111', '10')
--GO