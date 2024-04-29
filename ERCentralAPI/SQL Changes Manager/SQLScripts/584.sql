--First, create a user account for GPT4 if it doesn't exist already
USE [ReviewerAdmin]
GO

declare @GPTID int = (
	select CONTACT_ID from sTB_CONTACT 
	where CONTACT_NAME = 'OpenAI GPT4' and USERNAME = 'GPT4_BLOCKED' and DESCRIPTION = 'Stand-in account for the OpenAI GPT4 robot. Needs to remain "not activated" with EXPIRY_DATE = NULL' and EXPIRY_DATE is null
	)

IF @GPTID is null
BEGIN
	
	DECLARE @CONTACT_NAME nvarchar(255) = 'OpenAI GPT4'
	DECLARE @USERNAME nvarchar(50) = 'GPT4_BLOCKED'
	DECLARE @PASSWORD varchar(50) = 'NeverTo8Used!#!#'
	DECLARE @DATE_CREATED datetime = GETDATE()
	DECLARE @EMAIL nvarchar(500) = 'GPT4.FAKE@ucl.ac.uk'
	DECLARE @DESCRIPTION nvarchar(1000) = 'Stand-in account for the OpenAI GPT4 robot. Needs to remain "not activated" with EXPIRY_DATE = NULL'
	DECLARE @CONTACT_ID int

	EXECUTE [dbo].[st_ContactCreate] 
	   @CONTACT_NAME
	  ,@USERNAME
	  ,@PASSWORD
	  ,@DATE_CREATED
	  ,@EMAIL
	  ,@DESCRIPTION
	  ,@CONTACT_ID OUTPUT
END
GO

USE Reviewer
GO

--DELETE new table if it exists: TB_ROBOT_ACCOUNT
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_ACCOUNT')
BEGIN
	--also delete tables that depend on TB_ROBOT_ACCOUNT
	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_API_CALL_ERROR_LOG')	DROP TABLE [dbo].TB_ROBOT_API_CALL_ERROR_LOG
	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_API_CALL_LOG')	DROP TABLE [dbo].TB_ROBOT_API_CALL_LOG
	DROP TABLE [dbo].[TB_ROBOT_ACCOUNT]
END
GO

--New table for robot accounts
CREATE TABLE TB_ROBOT_ACCOUNT
(
	ROBOT_ID INT IDENTITY
	, CONTACT_ID INT FOREIGN KEY REFERENCES TB_CONTACT(CONTACT_ID)
	, FOR_SALE_IDs VARCHAR(2000) --comma separated list of IDs for sale, where order matters! 
	, CONSTRAINT PK_ROBOT_ID PRIMARY KEY(ROBOT_ID)
)

--by now, we know the GPT robot exists, get the ID and use it later
declare @GPTID int = (
	select CONTACT_ID from TB_CONTACT 
	where CONTACT_NAME = 'OpenAI GPT4' and USERNAME = 'GPT4_BLOCKED' and DESCRIPTION = 'Stand-in account for the OpenAI GPT4 robot. Needs to remain "not activated" with EXPIRY_DATE = NULL' and EXPIRY_DATE is null
	)

declare @forSale1 int, @forSale2 int;

--First line in ReviewerAdmin.dbo.TB_FOR_SALE
declare @chk int = (Select count(*) from ReviewerAdmin.dbo.TB_FOR_SALE where TYPE_NAME = 'GPT Input Tokens Per Million')
if @chk > 0 delete from ReviewerAdmin.dbo.TB_FOR_SALE where TYPE_NAME = 'GPT Input Tokens Per Million'
insert into ReviewerAdmin.dbo.TB_FOR_SALE (TYPE_NAME, IS_ACTIVE, LAST_CHANGED, PRICE_PER_MONTH, DETAILS)
	VALUES ('GPT Input Tokens Per Million', 1, GETDATE(), 25, 'Cost per million tokens sent to GPT API. In GBP, based on $30 price as of 22 Apr 2024')
set @forSale1 = SCOPE_IDENTITY()

--2nd line in ReviewerAdmin.dbo.TB_FOR_SALE
set @chk = (Select count(*) from ReviewerAdmin.dbo.TB_FOR_SALE where TYPE_NAME = 'GPT Output Tokens Per Million')
if @chk > 0 delete from ReviewerAdmin.dbo.TB_FOR_SALE where TYPE_NAME = 'GPT Output Tokens Per Million'
insert into ReviewerAdmin.dbo.TB_FOR_SALE (TYPE_NAME, IS_ACTIVE, LAST_CHANGED, PRICE_PER_MONTH, DETAILS)
	VALUES ('GPT Output Tokens Per Million', 1, GETDATE(), 50, 'Cost per million tokens received from GPT API. In GBP, based on $60 price as of 22 Apr 2024')
set @forSale2 = SCOPE_IDENTITY()

Insert into TB_ROBOT_ACCOUNT (CONTACT_ID, FOR_SALE_IDs) VALUES (@GPTID, CAST(@forSale1 as varchar(20)) + ',' + CAST(@forSale2 as varchar(20)))

IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_FOR_SALE')
 CREATE SYNONYM sTB_FOR_SALE FOR ReviewerAdmin.dbo.TB_FOR_SALE;

GO
Use [ReviewerAdmin]
GO

IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_ROBOT_ACCOUNT')
 CREATE SYNONYM sTB_ROBOT_ACCOUNT FOR Reviewer.dbo.TB_ROBOT_ACCOUNT;


--DELETE new table if it exists: TB_CREDIT_FOR_ROBOTS
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_CREDIT_FOR_ROBOTS')
BEGIN
	DROP TABLE [dbo].[TB_CREDIT_FOR_ROBOTS]
END
GO
--New table for linking reviews/SLs to the ability to use robots
CREATE TABLE TB_CREDIT_FOR_ROBOTS
(
	CREDIT_FOR_ROBOTS_ID INT IDENTITY
	, CREDIT_PURCHASE_ID INT FOREIGN KEY REFERENCES TB_CREDIT_PURCHASE(CREDIT_PURCHASE_ID)
	, REVIEW_ID int null 
	, LICENSE_ID int null
	, CONSTRAINT PK_CREDIT_FOR_ROBOTS_ID PRIMARY KEY(CREDIT_FOR_ROBOTS_ID)
)
GO

USE [Reviewer]
GO

IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_CREDIT_FOR_ROBOTS')
 CREATE SYNONYM sTB_CREDIT_FOR_ROBOTS FOR ReviewerAdmin.dbo.TB_CREDIT_FOR_ROBOTS;

/****** Object:  StoredProcedure [dbo].[st_ReviewInfo]    Script Date: 23/04/2024 11:45:14 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[st_ReviewInfo]
(
	@REVIEW_ID INT
	
)

As
BEGIN
	declare @chk int = ( 
						 select count(REVIEW_ID) FROM TB_REVIEW
						 where REVIEW_ID = @REVIEW_ID and SHOW_SCREENING = 1 and SCREENING_MODEL_RUNNING = 1
						)
    if @chk = 1 
	BEGIN
		--we want to check if we need to flip SCREENING_MODEL_RUNNING back to 0
		declare @t_id int
		select @t_id = max(training_id) from TB_TRAINING where REVIEW_ID = @REVIEW_ID
		if @t_id is not null
		BEGIN
			select @chk = count(training_id) from TB_TRAINING 
				where REVIEW_ID = @REVIEW_ID
				AND TRAINING_ID = @t_id 
				AND 
					(
					TIME_STARTED != TIME_ENDED
					OR
					GETDATE() > DATEADD(HOUR, 1, TIME_STARTED)
					)
				if @chk > 0 
					UPDATE TB_REVIEW set SCREENING_MODEL_RUNNING = 0 where REVIEW_ID = @REVIEW_ID
		END
	END

	SELECT * FROM TB_REVIEW	WHERE REVIEW_ID = @REVIEW_ID

	--prep for the second reader, returning info about credit being (possibly) available for use with robots - short and sweet, this needs to be fast
	select distinct a.CREDIT_PURCHASE_ID from 
	(select CREDIT_PURCHASE_ID from TB_SITE_LIC_REVIEW lr
		inner join sTB_CREDIT_FOR_ROBOTS cfr on lr.REVIEW_ID = @REVIEW_ID and cfr.LICENSE_ID = lr.SITE_LIC_ID
	UNION
		select CREDIT_PURCHASE_ID from sTB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID
	) as a
END
GO


USE [Reviewer]
GO

IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_CREDIT_PURCHASE')
 CREATE SYNONYM sTB_CREDIT_PURCHASE FOR ReviewerAdmin.dbo.TB_CREDIT_PURCHASE;

--DELETE new table if it exists: TB_ROBOT_API_CALL_LOG
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_API_CALL_LOG')
BEGIN
	IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_API_CALL_ERROR_LOG')	DROP TABLE [dbo].TB_ROBOT_API_CALL_ERROR_LOG
	DROP TABLE [dbo].TB_ROBOT_API_CALL_LOG
END
GO
--New table to record ROBOT usage
CREATE TABLE TB_ROBOT_API_CALL_LOG
(
	 ROBOT_API_CALL_ID INT IDENTITY
	, CREDIT_PURCHASE_ID INT 
	, REVIEW_ID int FOREIGN KEY REFERENCES TB_REVIEW(REVIEW_ID)
	, ROBOT_ID int FOREIGN KEY REFERENCES TB_ROBOT_ACCOUNT(ROBOT_ID)
	, REVIEW_SET_ID int FOREIGN KEY REFERENCES TB_REVIEW_SET(REVIEW_SET_ID)
	, CRITERIA varchar(max) not null
	, [STATUS] varchar(20)
	, CURRENT_ITEM_ID bigint
	, DATE_CREATED datetime
	, DATE_UPDATED datetime
	, SUCCESS bit
	, INPUT_TOKENS_COUNT int
	, OUTPUT_TOKENS_COUNT int
	, COST float
	, CONSTRAINT PK_ROBOT_API_CALL_ID PRIMARY KEY(ROBOT_API_CALL_ID)
)
GO
ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG] ADD  CONSTRAINT [DF_TB_ROBOT_API_CALL_LOG_DATE_CREATED]  DEFAULT (getdate()) FOR [DATE_CREATED]
GO
ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG] ADD  CONSTRAINT [DF_TB_ROBOT_API_CALL_LOG_DATE_UPDATED]  DEFAULT (getdate()) FOR [DATE_UPDATED]
GO
ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG] ADD  CONSTRAINT [DF_TB_ROBOT_API_CALL_LOG_INPUT_TOKENS_COUNT]  DEFAULT ((0)) FOR [INPUT_TOKENS_COUNT]
GO
ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG] ADD  CONSTRAINT [DF_TB_ROBOT_API_CALL_LOG_OUTPUT_TOKENS_COUNT]  DEFAULT ((0)) FOR [OUTPUT_TOKENS_COUNT]
GO
ALTER TABLE [dbo].[TB_ROBOT_API_CALL_LOG] ADD  CONSTRAINT [DF_TB_ROBOT_API_CALL_LOG_COST]  DEFAULT ((0.0)) FOR [COST]
GO



--DELETE new table if it exists: TB_ROBOT_API_CALL_ERROR_LOG
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_ROBOT_API_CALL_ERROR_LOG')
BEGIN
	DROP TABLE [dbo].[TB_ROBOT_API_CALL_ERROR_LOG]
END
GO
CREATE TABLE TB_ROBOT_API_CALL_ERROR_LOG
(
	ROBOT_API_CALL_ERROR_LOG_ID INT IDENTITY
	, ROBOT_API_CALL_ID INT FOREIGN KEY REFERENCES TB_ROBOT_API_CALL_LOG(ROBOT_API_CALL_ID)
	, [ERROR_MESSAGE] varchar(200)
	, STACK_TRACE varchar(max)
	, ITEM_ID bigint null
	, CONSTRAINT PK_ROBOT_API_CALL_ERROR_LOG_ID PRIMARY KEY(ROBOT_API_CALL_ERROR_LOG_ID)
)
GO

--NEW function to calculate how much ££ remains in a given credit purchase - will be used in all places where we do this calculation, so to have this logic implemented ONLY in one place
USE [ReviewerAdmin]
GO

IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sTB_ROBOT_API_CALL_LOG')
 CREATE SYNONYM sTB_ROBOT_API_CALL_LOG FOR Reviewer.dbo.TB_ROBOT_API_CALL_LOG;

/****** Object:  UserDefinedFunction [dbo].[fn_CreditRemainingDetails]    Script Date: 25/04/2024 11:03:59 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER FUNCTION [dbo].[fn_CreditRemainingDetails]
(
	@CREDIT_PURCHASE_ID int
)
RETURNS @tv_credit_purchase table (tv_credit_purchase_id int, tv_credit_purchaser_id int, tv_date_purchased date, tv_credit_purchased int,
		tv_credit_remaining float, tv_notes nvarchar(max), tv_contact_name nvarchar(255), tv_email nvarchar(500))
AS
BEGIN
	
	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)

	--OLD part, dealing with extensions of Accounts and Reviews
	insert into @tv_credit_purchase (tv_credit_purchase_id, tv_credit_purchaser_id, tv_date_purchased, tv_credit_purchased, tv_notes)
	select CREDIT_PURCHASE_ID, PURCHASER_CONTACT_ID, DATE_PURCHASED, CREDIT_PURCHASED, NOTES from TB_CREDIT_PURCHASE 
	where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int,  
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)


	declare @totalUsed int = 0
	declare @remainingCredit float = 0.0
	declare @creditPurchased int = 0


	set @creditPurchased = (select tv_credit_purchased from @tv_credit_purchase
		where tv_credit_purchase_id = @CREDIT_PURCHASE_ID)
		
	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
		tv_date_extended, tv_new_date, tv_old_date)
	select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
		eel.OLD_EXPIRY_DATE from TB_CREDIT_EXTENSIONS ce
	inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
	where ce.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	order by CREDIT_EXTENSION_ID

	-- reviews or accounts
	update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
	update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1

	-- get true old_date (i.e. whether the account/review was expired when it was extended)
	update @tv_credit_history set tv_true_old_date = tv_old_date
	update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date OR tv_old_date is null

	-- months extended
	update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

	-- add the total cost
	update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
	where tv_type_extended_name = 'Account'
	update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
	where tv_type_extended_name = 'Review'

	set @totalUsed = (SELECT SUM(tv_cost)
		FROM @tv_credit_history)
	IF @totalUsed is null set @totalUsed = 0
	set @remainingCredit = @creditPurchased - @totalUsed


	update @tv_credit_purchase
	set tv_contact_name = (select CONTACT_NAME from sTB_CONTACT 
	where CONTACT_ID = (select tv_credit_purchaser_id from @tv_credit_purchase))

	update @tv_credit_purchase
	set tv_email = (select EMAIL from sTB_CONTACT 
	where CONTACT_ID = (select tv_credit_purchaser_id from @tv_credit_purchase))

	--NEW part dealing with credit spent on Robots
	declare @robotUsed float = (select SUM(COST) from sTB_ROBOT_API_CALL_LOG where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID)
	if @robotUsed is null set @robotUsed = 0

	set @remainingCredit = @remainingCredit - @robotUsed
	update @tv_credit_purchase set tv_credit_remaining = @remainingCredit
	where tv_credit_purchase_id = @CREDIT_PURCHASE_ID


	RETURN
END
GO

USE Reviewer
GO
IF NOT EXISTS(SELECT * FROM sys.synonyms where name = 'sfn_CreditRemainingDetails')
 CREATE SYNONYM sfn_CreditRemainingDetails FOR ReviewerAdmin.dbo.fn_CreditRemainingDetails;
GO
USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_CreditPurchaseGet]    Script Date: 23/04/2024 13:51:22 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER       PROCEDURE [dbo].[st_CreditPurchaseGet] 
(
	@CREDIT_PURCHASE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select * from [dbo].[fn_CreditRemainingDetails](@CREDIT_PURCHASE_ID)	

    	
	RETURN
END
GO

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesByPurchaser]    Script Date: 25/04/2024 12:06:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER     PROCEDURE [dbo].[st_CreditPurchasesByPurchaser] 
(
	@CONTACT_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)


	declare @tv_credit_purchases table (tv_credit_purchase_id int, tv_date_purchased date, tb_credit_purchased int,
		tv_credit_remaining int)
	insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased, tv_credit_remaining)
	  SELECT tv_credit_purchase_id, tv_date_purchased, tv_credit_purchased, tv_credit_remaining 
	  from TB_CREDIT_PURCHASE cp
		Cross apply dbo.fn_CreditRemainingDetails(cp.CREDIT_PURCHASE_ID)  as details 
		where details.tv_credit_purchase_id = cp.CREDIT_PURCHASE_ID and cp.PURCHASER_CONTACT_ID = @CONTACT_ID
	--insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased)
	--select CREDIT_PURCHASE_ID, DATE_PURCHASED, CREDIT_PURCHASED from TB_CREDIT_PURCHASE 
	--where PURCHASER_CONTACT_ID = @CONTACT_ID

	--declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
	--	tv_type_extended_name nvarchar(1000), tv_id_extended int,  
	--	tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)


	--declare @totalUsed int = 0
	--declare @remainingCredit int = 0
	--declare @creditPurchased int = 0

	--declare @WORKING_CREDIT_PURCHASE_ID int
	--declare CREDIT_PURCHASE_ID_CURSOR cursor for
	--select tv_credit_purchase_id FROM @tv_credit_purchases
	--open CREDIT_PURCHASE_ID_CURSOR
	--fetch next from CREDIT_PURCHASE_ID_CURSOR
	--into @WORKING_CREDIT_PURCHASE_ID
	--while @@FETCH_STATUS = 0
	--begin
	--	set @creditPurchased = (select tb_credit_purchased from @tv_credit_purchases
	--		where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID)
		
	--	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
	--		tv_date_extended, tv_new_date, tv_old_date)
	--	select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
	--		eel.OLD_EXPIRY_DATE from TB_CREDIT_EXTENSIONS ce
	--	inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
	--	where ce.CREDIT_PURCHASE_ID = @WORKING_CREDIT_PURCHASE_ID
	--	order by CREDIT_EXTENSION_ID

	--	-- where a non-sharable review was extended and didn't have a tv_old_date
	--	update @tv_credit_history set tv_old_date = tv_date_extended where tv_old_date is null		

	--	-- reviews or accounts
	--	update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
	--	update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1

	--	-- get true old_date (i.e. whether the account/review was expired when it was extended)
	--	update @tv_credit_history set tv_true_old_date = tv_old_date
	--	update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date

	--	-- months extended
	--	update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

	--	-- add the total cost
	--	update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
	--	where tv_type_extended_name = 'Account'
	--	update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
	--	where tv_type_extended_name = 'Review'

	--	set @totalUsed = (SELECT SUM(tv_cost)
	--		FROM @tv_credit_history)

	--	set @remainingCredit = @creditPurchased - @totalUsed

	--	update @tv_credit_purchases set tv_credit_remaining = @remainingCredit
	--	where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID

	--	delete from @tv_credit_history

	--	set @totalUsed = 0
	--	set @remainingCredit = 0
	--	set @creditPurchased = 0

	--	FETCH NEXT FROM CREDIT_PURCHASE_ID_CURSOR 
	--	INTO @WORKING_CREDIT_PURCHASE_ID
	--END

	--CLOSE CREDIT_PURCHASE_ID_CURSOR
	--DEALLOCATE CREDIT_PURCHASE_ID_CURSOR

	select * from @tv_credit_purchases
       
END

GO

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 25/04/2024 13:38:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER       PROCEDURE [dbo].[st_CreditHistoryByPurchase] 
(
	@CREDIT_PURCHASE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int, tv_name nvarchar(255), 
		tv_date_extended datetime, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost float)

	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_type_extended_name, tv_id_extended, 
		tv_date_extended, tv_new_date, tv_old_date, tv_cost, tv_name)
	Select * from (
		select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, '' as type_extended_name, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
			eel.OLD_EXPIRY_DATE, 0 as tv_cost , '' as tv_name
		from TB_CREDIT_EXTENSIONS ce
		inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
		where ce.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
		--order by CREDIT_EXTENSION_ID
		UNION 
		select null, -1, c.CONTACT_NAME as type_extended_name, l.ROBOT_API_CALL_ID, l.DATE_CREATED, l.DATE_CREATED, 
		l.DATE_CREATED, l.COST as tv_cost, c.CONTACT_NAME as tv_name
		from sTB_ROBOT_API_CALL_LOG l
			inner join sTB_ROBOT_ACCOUNT ra on l.ROBOT_ID = ra.ROBOT_ID
			inner join sTB_CONTACT c on ra.CONTACT_ID = c.CONTACT_ID
			where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID and l.COST > 0
	) as a
	--order by DATE_OF_EDIT

	-- where a non-sharable review was extended and didn't have a tv_old_date
	update @tv_credit_history set tv_old_date = tv_date_extended where tv_old_date is null		

	-- reviews or accounts
	update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
	update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1
	update @tv_credit_history set tv_type_extended_name = 'Robot API Call' where tv_type_extended_id = -1

	-- review name
	update t1
	set tv_name = t2.REVIEW_NAME
	from @tv_credit_history t1 inner join sTB_REVIEW t2
	on t2.REVIEW_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Review'

	-- account name
	update t1
	set tv_name = t2.CONTACT_NAME
	from @tv_credit_history t1 inner join sTB_CONTACT t2
	on t2.CONTACT_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Account'

	-- get true old_date (i.e. whether the account/review was expired when it was extended)
	update @tv_credit_history set tv_true_old_date = tv_old_date
	update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date

	-- months extended
	update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

	-- add the total cost
	update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
	where tv_type_extended_name = 'Account'
	update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
	where tv_type_extended_name = 'Review'

	select * from @tv_credit_history order by tv_date_extended
       
	END

GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 25/04/2024 13:38:32 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE or ALTER PROCEDURE st_CreateRobotApiCallLog 
(
	@REVIEW_ID int,
	@CREDIT_PURCHASE_ID int,
	@ROBOT_NAME nvarchar(255),
	@CRITERIA varchar(max),
	@REVIEW_SET_ID int,
	@CURRENT_ITEM_ID bigint,
	@result varchar(50) OUTPUT, 
	@JobId int OUTPUT,
	@RobotContactId int OUTPUT
)
AS
BEGIN
	set @result = 'Success'
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	IF @rID is null OR @rID < 1
	BEGIN
		set @result = 'Robot not found';
		return;
	END
	set @RobotContactId = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	declare @chk int = (Select count(*) from  tb_review_set where REVIEW_SET_ID = @REVIEW_SET_ID and REVIEW_ID = @REVIEW_ID)
	if @chk < 1
	BEGIN
		set @result = 'Coding tool not found';
		return;
	END
	set @chk = (select count(*) from TB_REVIEW_CONTACT where REVIEW_ID = @REVIEW_ID and CONTACT_ID = @RobotContactId)
	if @chk < 1
	BEGIN
		declare @CR int 
		insert into TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID) values (@REVIEW_ID, @RobotContactId)
		SET @CR = SCOPE_IDENTITY()
		INSERT into TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME) values (@CR, 'Coding only')
	END

	insert into TB_ROBOT_API_CALL_LOG
		(CREDIT_PURCHASE_ID,REVIEW_ID,ROBOT_ID,REVIEW_SET_ID,CRITERIA,[STATUS], CURRENT_ITEM_ID ,SUCCESS,INPUT_TOKENS_COUNT,OUTPUT_TOKENS_COUNT,COST)
		VALUES
		(@CREDIT_PURCHASE_ID, @REVIEW_ID, @rID, @REVIEW_SET_ID, @CRITERIA, 'Starting', @CURRENT_ITEM_ID, 1, 0, 0, 0);
	SET @JobId = SCOPE_IDENTITY();
END
GO

CREATE or ALTER PROCEDURE st_UpdateRobotApiCallLog 
(
	@REVIEW_ID int,
	@ROBOT_API_CALL_ID int,
	@STATUS varchar(20),
	@CURRENT_ITEM_ID bigint,
	@INPUT_TOKENS_COUNT int = 0,
	@OUTPUT_TOKENS_COUNT int = 0,
	@ERROR_MESSAGE varchar(200) = '',
	@STACK_TRACE varchar(max) = ''
)
AS
BEGIN
	--first check that we're receiving data that makes sense
	declare @check int = (select count(*) from TB_ROBOT_API_CALL_LOG where REVIEW_ID = @REVIEW_ID and ROBOT_API_CALL_ID = @ROBOT_API_CALL_ID)
	if @check != 1 return -1;

	if @ERROR_MESSAGE != ''
	BEGIN
		Insert into TB_ROBOT_API_CALL_ERROR_LOG (ROBOT_API_CALL_ID, [ERROR_MESSAGE], STACK_TRACE, ITEM_ID)
			VALUES
			(@ROBOT_API_CALL_ID, @ERROR_MESSAGE, @STACK_TRACE, @CURRENT_ITEM_ID);
		UPDATE TB_ROBOT_API_CALL_LOG set SUCCESS = 0 WHERE ROBOT_API_CALL_ID = @ROBOT_API_CALL_ID;
	END
	declare @callCost float = 0.0
	declare @tt table (idx int, value bigint)
	declare @rID int = (select ROBOT_ID from TB_ROBOT_API_CALL_LOG where ROBOT_API_CALL_ID = @ROBOT_API_CALL_ID)
	declare @IdsTosplit varchar(2000) = (SELECT FOR_SALE_IDs from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	insert into @tt select * from dbo.fn_Split_int(@IdsTosplit, ',')
	declare @inputTokenCostPerMillion float = (select CAST(fs.PRICE_PER_MONTH as float) from sTB_FOR_SALE fs
												inner join @tt t on fs.FOR_SALE_ID = t.value and t.idx = 0)
	declare @outputTokenCostPerMillion float = (select CAST(fs.PRICE_PER_MONTH as float) from sTB_FOR_SALE fs
												inner join @tt t on fs.FOR_SALE_ID = t.value and t.idx = 1)

	set @callCost = (@inputTokenCostPerMillion * @INPUT_TOKENS_COUNT + @outputTokenCostPerMillion * @OUTPUT_TOKENS_COUNT)/1000000
	UPDATE TB_ROBOT_API_CALL_LOG set 
		CURRENT_ITEM_ID = @CURRENT_ITEM_ID
		, [STATUS] = @STATUS, DATE_UPDATED = GETDATE()
		, INPUT_TOKENS_COUNT = INPUT_TOKENS_COUNT + @INPUT_TOKENS_COUNT, OUTPUT_TOKENS_COUNT = OUTPUT_TOKENS_COUNT + @OUTPUT_TOKENS_COUNT
		, COST = COST + @callCost
		WHERE ROBOT_API_CALL_ID = @ROBOT_API_CALL_ID;
	

END
GO

CREATE or ALTER PROCEDURE st_ItemSetPrepareForRobot 
(
	@REVIEW_ID int,
	@ROBOT_CONTACT_ID int,
	@ITEM_ID bigint,
	@REVIEW_SET_ID int,
	@NEW_ITEM_SET_ID bigint OUTPUT
)
AS
BEGIN
	declare @SET_ID int = (select SET_ID from TB_REVIEW_SET where REVIEW_ID = @REVIEW_ID and REVIEW_SET_ID = @REVIEW_SET_ID)
	if @SET_ID is null OR @SET_ID < 1 
	BEGIN --didn't find a SET_ID, can't continue
		SET @NEW_ITEM_SET_ID = -1;
		return;
	END
	declare @check bigint = (SELECT ITEM_SET_ID from TB_ITEM_REVIEW ir
							INNER JOIN TB_ITEM_SET tis on ir.REVIEW_ID = @REVIEW_ID and ir.ITEM_ID = @ITEM_ID 
								AND ir.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @SET_ID and tis.CONTACT_ID = @ROBOT_CONTACT_ID)
	IF @check is NOT NULL
	BEGIN
		--ITEM_SET exists for this robot, SET and ITEM triplet, we make sure the coding is added as locked and then just return it
		UPDATE TB_ITEM_SET set IS_LOCKED = 1 where ITEM_SET_ID = @check
		set @NEW_ITEM_SET_ID = @check;
		return;
	END
	--there is no ITEM_SET for this robot, SET and ITEM triplet, need to create one. So we need to find out if it should be COMPLETED or not
	--RULES: ITEM_SET gets created as complete ONLY if the set is in "NORMAL data entry" (1) and does NOT have coding added by someone else already (2)
	--in all other cases, the coding made via ROBOTS gets added as incomplete
	DECLARE @IS_CODING_FINAL BIT
	SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE REVIEW_SET_ID = @REVIEW_SET_ID AND REVIEW_ID = @REVIEW_ID
	IF @IS_CODING_FINAL = 1 --coding tool is in normal mode Condition (1) is met
	BEGIN
		select @check = count(*) from TB_ITEM_REVIEW ir
							INNER JOIN TB_ITEM_SET tis on ir.REVIEW_ID = @REVIEW_ID and ir.ITEM_ID = @ITEM_ID 
								AND ir.ITEM_ID = tis.ITEM_ID and tis.SET_ID = @SET_ID and tis.CONTACT_ID != @ROBOT_CONTACT_ID
		IF @check > 0 set @IS_CODING_FINAL = 0 --Condition (2) in NOT met, we will add the Robot coding as incomplete.
	END
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID, IS_LOCKED)
	VALUES (@ITEM_ID, @SET_ID, @IS_CODING_FINAL, @ROBOT_CONTACT_ID, 1)
	SET @NEW_ITEM_SET_ID = SCOPE_IDENTITY()
END
GO

/****** Object:  StoredProcedure [dbo].[st_ItemAttributeInsert]    Script Date: 26/04/2024 09:06:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER procedure [dbo].[st_ItemAttributeInsert] (
	@ITEM_ID BIGINT,
	@SET_ID INT,
	@CONTACT_ID INT,
	@ATTRIBUTE_ID BIGINT,
	@ADDITIONAL_TEXT nvarchar(max),
	@REVIEW_ID INT,
	@ITEM_ARM_ID BIGINT, -- JT added item_arm_id 10/06/2018
	@ITEM_SET_ID BIGINT = NULL, --SG new optional param, for ROBOTS - April 2024

	@NEW_ITEM_ATTRIBUTE_ID BIGINT OUTPUT,
	@NEW_ITEM_SET_ID BIGINT OUTPUT
)

As
SET NOCOUNT ON
-- NORMAL route: ITEM_SET_ID is not provided:
-- First get a valid item_set_id.
-- If is_coding_final for this review then contact_id is irrelevant.
-- If coding is complete the contact_id is irrelevant.
-- Otherwise, we need a item_set_id for this specific contact.

-- ALT route for ROBOTS: ITEM_SET_ID is provided, to guarantee coding done by the ROBOT is always recorded as such

DECLARE @IS_CODING_FINAL BIT
--DECLARE @ITEM_SET_ID BIGINT = NULL
DECLARE @CHECK BIGINT

IF @ITEM_SET_ID is null
BEGIN --NORMAL route	
	SELECT @IS_CODING_FINAL = CODING_IS_FINAL FROM TB_REVIEW_SET WHERE SET_ID = @SET_ID AND REVIEW_ID = @REVIEW_ID

	SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND IS_COMPLETED = 'True'
	IF (@ITEM_SET_ID IS NULL)
	BEGIN
		SELECT @ITEM_SET_ID = ITEM_SET_ID FROM TB_ITEM_SET WHERE ITEM_ID = @ITEM_ID AND SET_ID = @SET_ID AND CONTACT_ID = @CONTACT_ID
	END
END
ELSE
BEGIN --ALT route for ROBOTS
	SELECT @IS_CODING_FINAL = IS_COMPLETED from TB_ITEM_SET WHERE ITEM_SET_ID = @ITEM_SET_ID and ITEM_ID = @ITEM_ID and SET_ID = @SET_ID
	IF @@ROWCOUNT = 0 OR @IS_CODING_FINAL is null
	BEGIN --@ITEM_SET_ID appears to be wrong! Can't continue
		SET @NEW_ITEM_SET_ID = -1;
		return; 
	END
END
	
IF (@ITEM_SET_ID IS NULL) -- have to create one 
BEGIN
	INSERT INTO TB_ITEM_SET(ITEM_ID, SET_ID, IS_COMPLETED, CONTACT_ID)
	VALUES (@ITEM_ID, @SET_ID, @IS_CODING_FINAL, @CONTACT_ID)
	SET @ITEM_SET_ID = SCOPE_IDENTITY()
END

-- We (finally) have an item_set_id we can use for our insert

-- JT modified 10/06/2018 to account for item arm ids too
-- SG modified 28/08/2018 we are passing NULL into @ITEM_ARM_ID when not adding to an arm, so need to do different thing
IF @ITEM_ARM_ID is null
begin 
	SELECT TOP(1) @CHECK = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID AND ITEM_SET_ID = @ITEM_SET_ID AND ITEM_ARM_ID is null
end
else
begin
	SELECT TOP(1) @CHECK = ITEM_ATTRIBUTE_ID FROM TB_ITEM_ATTRIBUTE WHERE ATTRIBUTE_ID = @ATTRIBUTE_ID AND ITEM_SET_ID = @ITEM_SET_ID AND ITEM_ARM_ID = @ITEM_ARM_ID
end

-- JT added item_arm_id
IF (@CHECK IS NULL) -- Not sure what to do if it's not null... - SHOULD REALLY THROW AN ERROR 
BEGIN
	INSERT INTO TB_ITEM_ATTRIBUTE(ITEM_ID, ITEM_SET_ID, ATTRIBUTE_ID, ADDITIONAL_TEXT, ITEM_ARM_ID)
	VALUES (@ITEM_ID, @ITEM_SET_ID, @ATTRIBUTE_ID, @ADDITIONAL_TEXT, @ITEM_ARM_ID)
	SET @NEW_ITEM_ATTRIBUTE_ID = @@IDENTITY 
END

SET @NEW_ITEM_SET_ID = @ITEM_SET_ID

SET NOCOUNT OFF
GO



--Link 2 credit purchases to two reviews
--INSERT into sTB_CREDIT_FOR_ROBOTS (CREDIT_PURCHASE_ID, REVIEW_ID, LICENSE_ID) values (9, 7, null), (10, 7, null), (9, 517, null), (10, 517, null)
--Link 1 credit purchase to one SITE License
--INSERT into sTB_CREDIT_FOR_ROBOTS (CREDIT_PURCHASE_ID, REVIEW_ID, LICENSE_ID) values (10, null, 3)