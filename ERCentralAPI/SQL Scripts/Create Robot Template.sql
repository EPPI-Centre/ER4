--This script is used to create a new LLM-robot for use in ER.
--The script needs to create records in different tables, in different DBs (ReviewerAdmin and Reviewer), 
--therefore to create a new, fully functional LLM-robot, you need to EDIT it in 2 places.
--Frist, it calls st_ContactCreate, in this part, you NEED to edit the CONTACT_NAME, USERNAME and EMAIL, and ensure they are unique.
--[If you get it wrong, the script will continue to the 2nd part, doing modifications that might not be intended!]
--Second, you need to edit values that will be entered into sTB_FOR_SALE. If you're working in production, these will affect how much using the LLM costs to end users!
--Third, you need to edit values that will be entered into TB_ROBOT_ACCOUNT, most important ones are ENDPOINT and PUBLIC_DESCRIPTION


-- TODO: Set parameter values below, you need to edit at least CONTACT_NAME, USERNAME and EMAIL, and ensure they are unique.
-- You can search for 'EDIT THIS' to find all values that need to be set!

USE [ReviewerAdmin]
GO

DECLARE @C_ID int, @RC int
declare @DATE_C datetime = GETDATE()
--GENERATE Random Password for the dummy account - we (nobody) needs to know it.
DECLARE @chars char(100) = '!т#$%&а()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~ийклм'
	declare @rnd varchar(50)
	declare @cnt int = 0
	set @rnd = ''
	WHILE (@cnt <= 20) 
	BEGIN
		SELECT @rnd = @rnd + 
			SUBSTRING(@chars, CONVERT(int, RAND() * 100), 1)
		SELECT @cnt = @cnt + 1
	END
--end of GENERATE random password

EXECUTE @RC = [dbo].[st_ContactCreate] 
   @CONTACT_NAME ='EDIT THIS' --for Example: 'Mistral Large 24.11'
  ,@USERNAME = 'EDIT THIS' --for example: 'Mistral-L24-11_BLOCKED'
  ,@PASSWORD = @rnd
  ,@DATE_CREATED = @DATE_C
  ,@EMAIL = 'EDIT THIS' --do not use a real email
  ,@DESCRIPTION = 'EDIT THIS' -- for example: 'Stand-in account for the Mistral Large 24.11 robot. Needs to remain "not activated" with EXPIRY_DATE = NULL'
  ,@CONTACT_ID = @C_ID OUTPUT
GO



Use Reviewer
GO
DECLARE @C_ID int = (SELECT max(CONTACT_ID) from TB_CONTACT)
declare @fsString varchar(100)

--EDIT TYPE_NAME, PRICE_PER_MONTH and DETAILS. PRICE_PER_MONTH affects users, the rest is for our internal record.
INSERT into sTB_FOR_SALE (TYPE_NAME, IS_ACTIVE, LAST_CHANGED, PRICE_PER_MONTH, DETAILS)
	values ('EDIT THIS', 1, GETDATE(), --for example: 'Mistral Large 24.11 Input Tokens Per Million'
	2, -- <- 'EDIT THIS'!!! This is PRICE_PER_MONTH, which actually stores "price per million tokens", find the true value in the Robot pages in Azure...
	'EDIT THIS') --For example: 'Cost per million tokens sent to Azure API. In GBP, based on $2 price as of 28 July 2025 for Mistral Large 24.11.'
set @fsString = cast(SCOPE_IDENTITY() as nvarchar(100)) + ','

--EDIT TYPE_NAME, PRICE_PER_MONTH and DETAILS. PRICE_PER_MONTH affects users, the rest is for our internal record.
INSERT into sTB_FOR_SALE (TYPE_NAME, IS_ACTIVE, LAST_CHANGED, PRICE_PER_MONTH, DETAILS) 
	values ('EDIT THIS', 1, GETDATE(), --for example: 'Mistral Large 24.11 Output Tokens Per Million'
	5, -- <- 'EDIT THIS'!!! This is PRICE_PER_MONTH, which actually stores "price per million tokens"
	'EDIT THIS') --for example: 'Cost per million tokens sent to Azure API. In GBP, based on $6 price as of 28 July 2025 for Mistral Large 24.11.'
set @fsString = @fsString + cast(SCOPE_IDENTITY() as nvarchar(100))

--See comments to decide what/how to edit!
INSERT into TB_ROBOT_ACCOUNT (CONTACT_ID, FOR_SALE_IDs, [ENDPOINT], IS_PUBLIC, REQUESTS_PER_MINUTE, PUBLIC_DESCRIPTION, RETIREMENT_DATE)
	VALUES(@C_ID, @fsString, 
	'EDIT THIS', --this is the [ENDPOINT], make sure it's correct!
	1, --IS_PUBLIC: 0 if visible to SiteAdmins only, 1 if visible to all
	60, --REQUESTS_PER_MINUTE: 150 is default, no need to exceed this. Might want a much smaller value in dev.
	'EDIT THIS', --PUBLIC_DESCRIPTION, this will be seen by users! For example 'The model used is "Mistral Large 24.11". This is a multilingual reasoning model, (mostly) open source.'
	CAST('EDIT THIS' AS DATETIME) --RETIREMENT_DATE!! if null, model never retires, otherwise it becomes disabled. For example '2026-04-11 00:00:00.000'
	)




declare @RobotId int 
select @RobotId = ROBOT_ID from TB_ROBOT_ACCOUNT r
inner join TB_CONTACT c on r.CONTACT_ID = c.CONTACT_ID and c.CONTACT_NAME = 'EDIT THIS' --MUST match what you used as CONTACT_NAME above
IF @RobotId is not null and @RobotId > 0
BEGIN
	declare @chk int = (select count(*) from TB_ROBOT_ACCOUNT_SETTING where ROBOT_ID = @RobotId)
	if @chk = 0
	begin
		INSERT INTO TB_ROBOT_ACCOUNT_SETTING (ROBOT_ID, SETTING_NAME, SETTING_VALUE) 
		VALUES (@RobotId, 'temperature', '0') --Might need to 'EDIT THIS' and all lines below! Depending on what parameters are allowed, etc...
		,(@RobotId, 'top_p', '1')
		,(@RobotId, 'response_format.type', 'json_object')
		,(@RobotId, 'random_seed', '1')
	END
END
GO



