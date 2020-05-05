use [ReviewerAdmin]

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_CREDIT_PURCHASE')
BEGIN
DROP TABLE [dbo].[TB_CREDIT_PURCHASE]
END
GO
CREATE TABLE [dbo].[TB_CREDIT_PURCHASE](
	[CREDIT_PURCHASE_ID] [int] IDENTITY(1,1) NOT NULL,
	[PURCHASER_CONTACT_ID] [int] NULL,
	[DATE_PURCHASED] [datetime] NULL,
	[CREDIT_PURCHASED] [int] NULL,
	[NOTES] [nvarchar](max) NULL,
	[PURCHASE_TYPE] [nvarchar](10) NULL,
 CONSTRAINT [PK_TB_INVOICED_CREDIT_PURCHASE] PRIMARY KEY CLUSTERED 
(
	[CREDIT_PURCHASE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


-----------------------------------------------

use [ReviewerAdmin]

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_CREDIT_EXTENSIONS')
BEGIN
DROP TABLE [dbo].[TB_CREDIT_EXTENSIONS]
END
GO
CREATE TABLE [dbo].[TB_CREDIT_EXTENSIONS](
	[CREDIT_EXTENSION_ID] [int] IDENTITY(1,1) NOT NULL,
	[CREDIT_PURCHASE_ID] [int] NULL,
	[EXPIRY_EDIT_ID] [int] NULL,
 CONSTRAINT [PK_TB_CREDIT_EXTENSIONS] PRIMARY KEY CLUSTERED 
(
	[CREDIT_EXTENSION_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

-----------------------------------------------

use [ReviewerAdmin]

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES
           WHERE TABLE_NAME = N'TB_OUTSTANDING_FEE')
BEGIN
DROP TABLE [dbo].[TB_OUTSTANDING_FEE]
END
GO
CREATE TABLE [dbo].[TB_OUTSTANDING_FEE](
	[OUTSTANDING_FEE_ID] [int] IDENTITY(1,1) NOT NULL,
	[ACCOUNT_ID] [int] NULL,
	[DATE_CREATED] [datetime] NULL,
	[AMOUNT] [int] NULL,
	[NOTES] [nvarchar](max) NULL,
	[STATUS] [nvarchar](15) NOT NULL,
 CONSTRAINT [PK_TB_OUTSTANDING_FEE] PRIMARY KEY CLUSTERED 
(
	[OUTSTANDING_FEE_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TB_OUTSTANDING_FEE] ADD  CONSTRAINT [DF_TB_OUTSTANDING_FEE_STATUS]  DEFAULT (N'Outstanding') FOR [STATUS]
GO

--------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OutstandingFeesGetAll]    Script Date: 20/03/2020 14:28:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_OutstandingFeesGetAll] 
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select  o_f.OUTSTANDING_FEE_ID, o_f.DATE_CREATED, o_f.ACCOUNT_ID, c.CONTACT_NAME, 
	c.EMAIL, o_f.AMOUNT, o_f.[STATUS] 
	from TB_OUTSTANDING_FEE o_f
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = o_f.ACCOUNT_ID
	where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
			(c.EMAIL like '%' + @TEXT_BOX + '%'))

	group by o_f.OUTSTANDING_FEE_ID, o_f.DATE_CREATED, o_f.ACCOUNT_ID, c.CONTACT_NAME, 
	c.EMAIL, o_f.AMOUNT, o_f.[STATUS] 
       
END



GO

------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OutstandingFeeGet]    Script Date: 20/03/2020 14:29:44 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






CREATE OR ALTER PROCEDURE [dbo].[st_OutstandingFeeGet] 
(
	@OUTSTANDING_FEE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	select  o_f.DATE_CREATED, o_f.ACCOUNT_ID, c.CONTACT_NAME, 
	c.EMAIL, o_f.AMOUNT, o_f.[STATUS], o_f.NOTES from TB_OUTSTANDING_FEE o_f
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = o_f.ACCOUNT_ID
	where o_f.OUTSTANDING_FEE_ID = @OUTSTANDING_FEE_ID
    	
	RETURN
END


GO

----------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OutstandingFeeCreateOrEdit]    Script Date: 20/03/2020 14:32:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_OutstandingFeeCreateOrEdit]
(
	@OUTSTANDING_FEE_ID int,
	@ACCOUNT_ID int,
	@AMOUNT int,
	@DATE_CREATED datetime,
	@NOTES nvarchar(max)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @NOTES = 'DeleteThisFee'
	begin
		delete from TB_OUTSTANDING_FEE where OUTSTANDING_FEE_ID = @OUTSTANDING_FEE_ID
	end
	else
	begin
		if @OUTSTANDING_FEE_ID = 0
		begin
		insert into TB_OUTSTANDING_FEE (ACCOUNT_ID, AMOUNT, DATE_CREATED, NOTES, [STATUS])
			values (@ACCOUNT_ID, @AMOUNT, @DATE_CREATED, @NOTES, 'Outstanding')
		end
		else
		begin
			update TB_OUTSTANDING_FEE
			set ACCOUNT_ID = @ACCOUNT_ID, AMOUNT = @AMOUNT, DATE_CREATED = @DATE_CREATED, NOTES = @NOTES
			where OUTSTANDING_FEE_ID = @OUTSTANDING_FEE_ID

		end
	end

END


GO

------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_OutstandingFeeByAccountID]    Script Date: 20/03/2020 14:32:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_OutstandingFeeByAccountID] 
(
	@CONTACT_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select * from TB_OUTSTANDING_FEE
	where ACCOUNT_ID = @CONTACT_ID
	and STATUS like 'Outstanding'
       
END



GO

----------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillAddOutstandingFee]    Script Date: 20/03/2020 14:33:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillAddOutstandingFee]
	-- Add the parameters for the stored procedure here
	@bill_ID int,
	@OutstandingFeeAmount int,
	@Result nvarchar(100) = 'Success' out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	declare @numberMonths int
	 
	set @Result = 'Success'

	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Outstanding fee'
	order by LAST_CHANGED desc

	declare @pricePerMonth int = (select PRICE_PER_MONTH from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')

	set @numberMonths = @OutstandingFeeAmount / @pricePerMonth
	
	-- if there is already an outstanding fee in the bill then it is an update
	select * from TB_BILL_LINE
	where BILL_ID = @BILL_ID
	and FOR_SALE_ID = @FOR_SALE_ID
	if @@ROWCOUNT = 0
	begin	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@bill_ID, @FOR_SALE_ID, 0, @numberMonths)
		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	end
	else
	begin
		update TB_BILL_LINE
		set MONTHS = @numberMonths
		where BILL_ID = @bill_ID
		and FOR_SALE_ID = @FOR_SALE_ID
	end
		
	
	
	RETURN
END
GO

---------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillDetailsOutstandingFees]    Script Date: 20/03/2020 14:33:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillDetailsOutstandingFees] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ForSaleID int
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')

	select b_l.LINE_ID, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on b_l.FOR_SALE_ID = f_s.FOR_SALE_ID
	where b_l.BILL_ID = @BILL_ID
	and f_s.FOR_SALE_ID = @ForSaleID
	RETURN

END
GO

-------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesGetAll]    Script Date: 20/03/2020 14:34:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CreditPurchasesGetAll] 
(
	@TEXT_BOX nvarchar(255)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	select cp.CREDIT_PURCHASE_ID, cp.DATE_PURCHASED, cp.PURCHASER_CONTACT_ID, c.CONTACT_NAME, 
	c.EMAIL, cp.CREDIT_PURCHASED, cp.PURCHASE_TYPE 
	from TB_CREDIT_PURCHASE cp
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = cp.PURCHASER_CONTACT_ID
	where ((c.CONTACT_ID like '%' + @TEXT_BOX + '%') OR
			(c.CONTACT_NAME like '%' + @TEXT_BOX + '%') OR
			(c.EMAIL like '%' + @TEXT_BOX + '%'))

	group by cp.CREDIT_PURCHASE_ID, cp.DATE_PURCHASED, cp.PURCHASER_CONTACT_ID, 
	c.CONTACT_NAME, cp.CREDIT_PURCHASED, c.EMAIL, cp.PURCHASE_TYPE
       
END



GO

----------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesByPurchaser]    Script Date: 20/03/2020 14:35:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CreditPurchasesByPurchaser] 
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
	insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased)
	select CREDIT_PURCHASE_ID, DATE_PURCHASED, CREDIT_PURCHASED from TB_CREDIT_PURCHASE 
	where PURCHASER_CONTACT_ID = @CONTACT_ID

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int,  
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)


	declare @totalUsed int = 0
	declare @remainingCredit int = 0
	declare @creditPurchased int = 0

	declare @WORKING_CREDIT_PURCHASE_ID int
	declare CREDIT_PURCHASE_ID_CURSOR cursor for
	select tv_credit_purchase_id FROM @tv_credit_purchases
	open CREDIT_PURCHASE_ID_CURSOR
	fetch next from CREDIT_PURCHASE_ID_CURSOR
	into @WORKING_CREDIT_PURCHASE_ID
	while @@FETCH_STATUS = 0
	begin
		set @creditPurchased = (select tb_credit_purchased from @tv_credit_purchases
			where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID)
		
		insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
			tv_date_extended, tv_new_date, tv_old_date)
		select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
			eel.OLD_EXPIRY_DATE from TB_CREDIT_EXTENSIONS ce
		inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
		where ce.CREDIT_PURCHASE_ID = @WORKING_CREDIT_PURCHASE_ID
		order by CREDIT_EXTENSION_ID

		-- reviews or accounts
		update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
		update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1

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

		set @totalUsed = (SELECT SUM(tv_cost)
			FROM @tv_credit_history)

		set @remainingCredit = @creditPurchased - @totalUsed

		update @tv_credit_purchases set tv_credit_remaining = @remainingCredit
		where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID

		set @totalUsed = 0
		set @remainingCredit = 0
		set @creditPurchased = 0

		FETCH NEXT FROM CREDIT_PURCHASE_ID_CURSOR 
		INTO @WORKING_CREDIT_PURCHASE_ID
	END

	CLOSE CREDIT_PURCHASE_ID_CURSOR
	DEALLOCATE CREDIT_PURCHASE_ID_CURSOR

	select * from @tv_credit_purchases
       
END



GO

-----------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchaseGet]    Script Date: 20/03/2020 14:35:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE OR ALTER PROCEDURE [dbo].[st_CreditPurchaseGet] 
(
	@CREDIT_PURCHASE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	select cp.PURCHASER_CONTACT_ID, cp.DATE_PURCHASED, cp.CREDIT_PURCHASED, 
	     cp.NOTES, c.CONTACT_NAME, c.EMAIL from TB_CREDIT_PURCHASE cp
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = cp.PURCHASER_CONTACT_ID
	where cp.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
    	
	RETURN
END


GO

-------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchaseCreateOrEdit]    Script Date: 20/03/2020 14:36:03 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_CreditPurchaseCreateOrEdit]
(
	@CREDIT_PURCHASE_ID int,
	@CONTACT_ID int,
	@CREDIT_PURCHASED int,
	@DATE_PURCHASED datetime,
	@NOTES nvarchar(max),
	@PURCHASE_TYPE nvarchar(10)
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @CREDIT_PURCHASE_ID = 0
	begin
	insert into TB_CREDIT_PURCHASE (PURCHASER_CONTACT_ID, CREDIT_PURCHASED, DATE_PURCHASED, NOTES, PURCHASE_TYPE)
		values (@CONTACT_ID, @CREDIT_PURCHASED, @DATE_PURCHASED, @NOTES, @PURCHASE_TYPE)
	end
	else
	begin
		update TB_CREDIT_PURCHASE
		set PURCHASER_CONTACT_ID = @CONTACT_ID, CREDIT_PURCHASED = @CREDIT_PURCHASED,
		    DATE_PURCHASED = @DATE_PURCHASED, NOTES = @NOTES, PURCHASE_TYPE = @PURCHASE_TYPE
		where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID

	end

END


GO

-------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillAddCredit]    Script Date: 20/03/2020 14:36:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillAddCredit]
	-- Add the parameters for the stored procedure here
	@bill_ID int,
	@CreditAmount int,
	@Result nvarchar(100) = 'Success' out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	declare @numberMonths int
	 
	set @Result = 'Success'

	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Credit purchase'
	order by LAST_CHANGED desc

	declare @pricePerMonth int = (select PRICE_PER_MONTH from TB_FOR_SALE where TYPE_NAME = 'Credit purchase')
	set @numberMonths = @CreditAmount / @pricePerMonth
	
	-- if there is already a credit purchase in the bill then it is an update
	select * from TB_BILL_LINE
	where BILL_ID = @BILL_ID
	and FOR_SALE_ID = @FOR_SALE_ID
	if @@ROWCOUNT = 0
	begin	
		insert into TB_BILL_LINE (BILL_ID, FOR_SALE_ID, AFFECTED_ID, MONTHS)
		VALUES (@bill_ID, @FOR_SALE_ID, 0, @numberMonths)
		if @@ROWCOUNT != 1 set @Result = 'Unknown Error, please contact Support.'
	end
	else
	begin
		update TB_BILL_LINE
		set MONTHS = @numberMonths
		where BILL_ID = @bill_ID
		and FOR_SALE_ID = @FOR_SALE_ID
	end
		
	
	
	RETURN
END
GO

-----------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillDetailsCredit]    Script Date: 20/03/2020 14:36:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_BillDetailsCredit] 
(
	@BILL_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	declare @ForSaleID int
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Credit purchase')

	select b_l.LINE_ID, (f_s.PRICE_PER_MONTH * b_l.MONTHS) as COST
	from TB_BILL_LINE b_l
	inner join TB_FOR_SALE f_s
	on b_l.FOR_SALE_ID = f_s.FOR_SALE_ID
	where b_l.BILL_ID = @BILL_ID
	and f_s.FOR_SALE_ID = @ForSaleID
	RETURN

END
GO

--------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ApplyCreditToReview]    Script Date: 20/03/2020 14:37:35 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER procedure [dbo].[st_ApplyCreditToReview]
(
	@CREDIT_PURCHASE_ID int,
	@REVIEW_ID int,
	@MONTHS_EXTENDED int,
	@PERSON_EXTENDING_CONTACT_ID int
)

As

SET NOCOUNT ON

	-- extend the review by the selected number of months
	
	-- is the review activated
	declare @monthsCredit int
	declare @oldExpiryDate date
	declare @newExpiryDate date
	declare @newExpiryEditID int

	set @monthsCredit = (select MONTHS_CREDIT from Reviewer.dbo.TB_REVIEW
	where REVIEW_ID = @REVIEW_ID)

	declare @extensionTypeID int = (select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Using credit purchase')

	if @monthsCredit > 0
	begin
		-- this is an unactivated review so increase the months of credit
		set @monthsCredit = @monthsCredit + @MONTHS_EXTENDED
		update Reviewer.dbo.TB_REVIEW set MONTHS_CREDIT = @monthsCredit
		where REVIEW_ID = @REVIEW_ID
		set @oldExpiryDate = null
		set @newExpiryDate = null
	end
	else
	begin
		set @oldExpiryDate = (select EXPIRY_DATE from Reviewer.dbo.TB_REVIEW
			where REVIEW_ID = @REVIEW_ID)
		if @oldExpiryDate is null
		begin
			-- this is a non-shareable review so make it shareable 
			update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
			where REVIEW_ID = @REVIEW_ID
			set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
		end
		else
		begin
			-- this is a shareable review so extend the expiry date
			if @oldExpiryDate <= getdate()
			begin
				-- it expired so add from today
				update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
				where REVIEW_ID = @REVIEW_ID
				set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
			end
			else
			begin
				-- not yet expired so add from old expiry date
				update Reviewer.dbo.TB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
				where REVIEW_ID = @REVIEW_ID
				set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
			end
		end
	end



	-- add the extension to TB_EXPIRY_EDIT_LOG
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 0, @REVIEW_ID, @oldExpiryDate, @newExpiryDate, @PERSON_EXTENDING_CONTACT_ID, @extensionTypeID,
		'Extended using a credit purchase')
	SET @newExpiryEditID = @@IDENTITY

	-- add entry into TB_CREDIT_EXTENSIONS
	insert into TB_CREDIT_EXTENSIONS (CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
	values (@CREDIT_PURCHASE_ID, @newExpiryEditID)
		

SET NOCOUNT OFF

GO

----------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ApplyCreditToAccount]    Script Date: 20/03/2020 14:37:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER procedure [dbo].[st_ApplyCreditToAccount]
(
	@CREDIT_PURCHASE_ID int,
	@CONTACT_ID int,
	@MONTHS_EXTENDED int,
	@PERSON_EXTENDING_CONTACT_ID int
)

As

SET NOCOUNT ON

	-- extend the review by the selected number of months
	
	-- is the review activated
	declare @monthsCredit int
	declare @oldExpiryDate date
	declare @newExpiryDate date
	declare @newExpiryEditID int

	set @monthsCredit = (select MONTHS_CREDIT from Reviewer.dbo.TB_CONTACT
	where CONTACT_ID = @CONTACT_ID)

	declare @extensionTypeID int = (select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Using credit purchase')

	if @monthsCredit > 0
	begin
		-- this is an unactivated user account so increase the months of credit
		set @monthsCredit = @monthsCredit + @MONTHS_EXTENDED
		update Reviewer.dbo.TB_CONTACT set MONTHS_CREDIT = @monthsCredit
		where CONTACT_ID = @CONTACT_ID
		set @oldExpiryDate = null
		set @newExpiryDate = null
	end
	else
	begin
		set @oldExpiryDate = (select EXPIRY_DATE from Reviewer.dbo.TB_CONTACT
			where CONTACT_ID = @CONTACT_ID)
		if @oldExpiryDate <= getdate()
		begin
			-- it expired so add from today
			update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
				where CONTACT_ID = @CONTACT_ID
			set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
		end
		else
		begin
			-- not yet expired so add from old expiry date
			update Reviewer.dbo.TB_CONTACT set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
				where CONTACT_ID = @CONTACT_ID
			set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
		end
	end

	-- add the extension to TB_EXPIRY_EDIT_LOG
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
		EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 1, @CONTACT_ID, @oldExpiryDate, @newExpiryDate, @PERSON_EXTENDING_CONTACT_ID, @extensionTypeID,
		'Extended using a credit purchase')
	SET @newExpiryEditID = @@IDENTITY

	-- add entry into TB_CREDIT_EXTENSIONS
	insert into TB_CREDIT_EXTENSIONS (CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
	values (@CREDIT_PURCHASE_ID, @newExpiryEditID)
		

SET NOCOUNT OFF

GO

------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 20/03/2020 14:38:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_CreditHistoryByPurchase] 
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
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)

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

	-- review name
	update t1
	set tv_name = t2.REVIEW_NAME
	from @tv_credit_history t1 inner join Reviewer.dbo.TB_REVIEW t2
	on t2.REVIEW_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Review'

	-- account name
	update t1
	set tv_name = t2.CONTACT_NAME
	from @tv_credit_history t1 inner join Reviewer.dbo.TB_CONTACT t2
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

	select * from @tv_credit_history
       
	END



GO

-----------------------------------

Use ReviewerAdmin
GO

select * from TB_FOR_SALE
where [TYPE_NAME] = 'Credit purchase'
if @@ROWCOUNT = 0
begin
	insert into TB_FOR_SALE ([TYPE_NAME], IS_ACTIVE, LAST_CHANGED, PRICE_PER_MONTH, DETAILS)
	VALUES ('Credit purchase', 1, getdate(), 5, 'A credit purchase')
end
GO

-------------------------------------

Use ReviewerAdmin
GO

select * from TB_FOR_SALE
where [TYPE_NAME] = 'Outstanding fee'
if @@ROWCOUNT = 0
begin
	insert into TB_FOR_SALE ([TYPE_NAME], IS_ACTIVE, LAST_CHANGED, PRICE_PER_MONTH, DETAILS)
	VALUES ('Outstanding fee', 1, getdate(), 5, 'This is an outstanding fee for previous extensions')
end
GO

--------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillGetDraft]    Script Date: 20/03/2020 14:54:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_BillGetDraft]
(
	@CONTACT int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--first, mark as expired the submitted bills that did not end up with success or failure
	update TB_BILL set BILL_STATUS = 'submitted to WPM: timed out'
		where BILL_STATUS = 'submitted to WPM' and PURCHASER_CONTACT_ID = @CONTACT
    -- get the details from tb_bill
	select c.CONTACT_NAME, b.BILL_ID, b.DATE_PURCHASED, b.NOMINAL_PRICE, b.DISCOUNT,
	b.DUE_PRICE, b.CONDITIONS_ID, b.BILL_STATUS, b.DATE_PAYMENT_RECEIVED,
	b.PURCHASER_CONTACT_ID, b.VAT
	from TB_BILL b
	inner join Reviewer.dbo.TB_CONTACT c
	on b.PURCHASER_CONTACT_ID = c.CONTACT_ID AND c.CONTACT_ID = @CONTACT
	and b.BILL_STATUS = 'Draft'
	--second reader get the bill lines
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, c.CONTACT_NAME as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Professional'
		left outer join Reviewer.dbo.TB_CONTACT c on bl.AFFECTED_ID = c.CONTACT_ID and bl.AFFECTED_ID is not null
	UNION
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, r.REVIEW_NAME as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
		left outer join Reviewer.dbo.TB_REVIEW r on bl.AFFECTED_ID = r.REVIEW_ID and bl.AFFECTED_ID is not null
	UNION
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, '' as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Credit purchase'
	UNION
	select LINE_ID, bl.BILL_ID, bl.FOR_SALE_ID, fs.TYPE_NAME, AFFECTED_ID, MONTHS MONTHS_CREDIT, b.PURCHASER_CONTACT_ID, PRICE_PER_MONTH * MONTHS COST
	, '' as AFFECTED_NAME
	From TB_BILL_LINE bl inner join TB_BILL b 
		on bl.BILL_ID = b.BILL_ID and b.BILL_STATUS = 'Draft' and b.PURCHASER_CONTACT_ID = @CONTACT
		inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Outstanding fee'
	RETURN

END

GO

-----------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillMarkAsPaid]    Script Date: 20/03/2020 14:55:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_BillMarkAsPaid]
	-- Add the parameters for the stored procedure here
	@BILL_ID int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    --to be VERY sure this doesn't happen in part, we nest a transaction inside a try catch clause
	BEGIN TRY   
	BEGIN TRANSACTION

------------------------------------------------------------------------------------------------------------	 
	-- Existing Accounts	
		
	 --1a.collect data for TB_EXPIRY_EDIT_LOG
	 CREATE TABLE #ExistingAccounts(DATE_OF_EDIT datetime, TYPE_EXTENDED int, ID_EXTENDED int, 
	 NEW_EXPIRY_DATE date, OLD_EXPIRY_DATE date, LENGTH_OF_EXTENSION int, EXTENDED_BY_ID int, 
	 EXTENSION_TYPE_ID int)

	 INSERT INTO #ExistingAccounts (DATE_OF_EDIT, ID_EXTENDED, TYPE_EXTENDED, EXTENSION_TYPE_ID, LENGTH_OF_EXTENSION)
	 Select GETDATE(),  AFFECTED_ID as ID_EXTENDED, '1', '2', bl.MONTHS from TB_BILL_LINE bl
	 inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
	 and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
	 -- TYPE_EXTENDED = 1 for contacts
	 
	 update #ExistingAccounts set OLD_EXPIRY_DATE =
	 (select c.EXPIRY_DATE from [Reviewer].[dbo].[TB_CONTACT] c 
	 where c.CONTACT_ID = #ExistingAccounts.ID_EXTENDED)

	 update #ExistingAccounts set EXTENDED_BY_ID =
	 (select PURCHASER_CONTACT_ID from TB_BILL b
	 inner join [Reviewer].[dbo].[TB_CONTACT] c on c.CONTACT_ID = b.PURCHASER_CONTACT_ID
	 where b.BILL_ID = @bill_ID)
	
	 --1b.extend existing accounts
	 update Reviewer.dbo.TB_CONTACT set 
		[EXPIRY_DATE] = case 
			when ([EXPIRY_DATE] is null) then null
			when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
			ELSE DATEADD(month, a.MONTHS, getdate())
		end
		, MONTHS_CREDIT = case when (EXPIRY_DATE is null and  MONTHS_CREDIT is null)
			then MONTHS
			when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
		else 0
		end
		from (
				Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
				and BILL_ID = @bill_ID and fs.TYPE_NAME = 'professional' and AFFECTED_ID is not null
			) a
	 where CONTACT_ID = a.AFFECTED_ID
	 
	 --1c. update TB_EXPIRY_EDIT_LOG
	 update #ExistingAccounts set NEW_EXPIRY_DATE =
	 (select c.EXPIRY_DATE from [Reviewer].[dbo].[TB_CONTACT] c 
	 where c.CONTACT_ID = #ExistingAccounts.ID_EXTENDED)
	 
	 
	 insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID)
	 select DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID
	 from #ExistingAccounts
	 
	 drop table #ExistingAccounts
	 
------------------------------------------------------------------------------------------------------------	 
	
	-- Existing Reviews
	 
	 --2a.collect data for TB_EXPIRY_EDIT_LOG
	 CREATE TABLE #ExistingReviews(DATE_OF_EDIT datetime, TYPE_EXTENDED int, ID_EXTENDED int, 
	 NEW_EXPIRY_DATE date, OLD_EXPIRY_DATE date, LENGTH_OF_EXTENSION int, EXTENDED_BY_ID int, 
	 EXTENSION_TYPE_ID int)

	 INSERT INTO #ExistingReviews (DATE_OF_EDIT, ID_EXTENDED, TYPE_EXTENDED, EXTENSION_TYPE_ID, LENGTH_OF_EXTENSION)
	 Select GETDATE(),  AFFECTED_ID as ID_EXTENDED, '0', '2', bl.MONTHS from TB_BILL_LINE bl
	 inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
	 and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
	 -- TYPE_EXTENDED = 0 for reviews

	 update #ExistingReviews set OLD_EXPIRY_DATE =
	 (select r.EXPIRY_DATE from [Reviewer].[dbo].[TB_REVIEW] r 
	 where r.REVIEW_ID = #ExistingReviews.ID_EXTENDED)

	 update #ExistingReviews set EXTENDED_BY_ID = PURCHASER_CONTACT_ID from TB_BILL b
	 where b.BILL_ID = @bill_ID
	 
	 --2b.extend existing reviews
	 update Reviewer.dbo.TB_REVIEW set 
		[EXPIRY_DATE] = case 
			When ([EXPIRY_DATE] is null) then null
			when ([EXPIRY_DATE] > getdate()) then DATEADD(month, a.MONTHS, [EXPIRY_DATE])
			ELSE DATEADD(month, a.MONTHS, getdate())
		end
		, MONTHS_CREDIT = Case When (EXPIRY_DATE is null and MONTHS_CREDIT is null) then a.MONTHS
		when (EXPIRY_DATE is null and MONTHS_CREDIT is not null) then MONTHS_CREDIT + a.MONTHS
		else 0
		end
		from (
				Select AFFECTED_ID, bl.MONTHS from TB_BILL_LINE bl
				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID
				and BILL_ID = @bill_ID and fs.TYPE_NAME = 'Shareable Review' and AFFECTED_ID is not null
			) a
	 where REVIEW_ID = a.AFFECTED_ID
	 
	 --2c. update TB_EXPIRY_EDIT_LOG
	 update #ExistingReviews set NEW_EXPIRY_DATE =
	 (select r.EXPIRY_DATE from [Reviewer].[dbo].[TB_REVIEW] r 
	 where r.REVIEW_ID = #ExistingReviews.ID_EXTENDED)
	 
	 
	 insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID)
	 select DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
		OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID
	 from #ExistingReviews
	 
	 drop table #ExistingReviews
	 
------------------------------------------------------------------------------------------------------------	 
	 
	 --3.create accounts
		declare @bl int
		declare cr cursor FAST_FORWARD
		for select LINE_ID from tb_bill b
			inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'professional'
		open cr
		fetch next from cr into @bl
		while @@fetch_status=0
		begin
			 insert into Reviewer.dbo.TB_CONTACT (CONTACT_NAME, [DATE_CREATED], [EXPIRY_DATE], 
				MONTHS_CREDIT, CREATOR_ID, [TYPE], IS_SITE_ADMIN)
				Select Null ,getdate(), Null, MONTHS + 1, PURCHASER_CONTACT_ID, 'Professional', 0
					from TB_BILL b 
						inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
			update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
			fetch next from cr into @bl
		end
		close cr
		deallocate cr
	
			
	 --4.create reviews
		declare cr cursor FAST_FORWARD
		for select LINE_ID from tb_bill b
			inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID
				inner join TB_FOR_SALE fs on bl.FOR_SALE_ID = fs.FOR_SALE_ID and fs.TYPE_NAME = 'Shareable Review'
		open cr
		fetch next from cr into @bl
		while @@fetch_status=0
		begin
			 insert into Reviewer.dbo.TB_REVIEW (REVIEW_NAME, [DATE_CREATED], [EXPIRY_DATE], 
				MONTHS_CREDIT, FUNDER_ID)
				select Null, GETDATE(), Null, MONTHS, PURCHASER_CONTACT_ID
				from TB_BILL b 
					inner join TB_BILL_LINE bl on b.BILL_ID = bl.BILL_ID and bl.AFFECTED_ID is null and b.BILL_ID = @bill_ID and LINE_ID = @bl
			update TB_BILL_LINE set AFFECTED_ID = @@IDENTITY where LINE_ID = @bl
			fetch next from cr into @bl
		end
		close cr
		deallocate cr

---------------------------------------------------------------------------------------------

	--5. mark any outstanding fees as paid
	-- there should only be one line for outstanding fees in the bill as they get added up and put in one line
	declare @PurchaserContactID int
	set @PurchaserContactID = (select PURCHASER_CONTACT_ID from TB_BILL where BILL_ID = @bill_ID)
	declare @ForSaleID int
	set @ForSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Outstanding fee')

	select * from TB_BILL_LINE
	where BILL_ID = @bill_ID
	and FOR_SALE_ID = @ForSaleID
	if @@ROWCOUNT > 0
	begin
		-- there was an outstanding fee in the bill so mark them as paid in TB_OUTSTANDING_FEE
		update TB_OUTSTANDING_FEE
		set STATUS = 'Paid'
		where ACCOUNT_ID = @PurchaserContactID
		and STATUS like 'Outstanding'
	end


	--6.change bill to paid
	update TB_BILL set BILL_STATUS = 'OK: Paid and data committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
	COMMIT TRANSACTION
	END TRY

	BEGIN CATCH
	IF (@@TRANCOUNT > 0) 
		BEGIN 
			--error corrections: 1.undo all changes
			ROLLBACK TRANSACTION
			--2.mark bill appropriately
			update TB_BILL set BILL_STATUS = 'FAILURE: paid but data NOT committed', DATE_PURCHASED = GETDATE() where BILL_ID = @bill_ID
		END 
	END CATCH

END


GO

--------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewMembers]    Script Date: 20/03/2020 15:00:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReviewMembers] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @tb_review_members table 
		(contact_id int, review_contact_id int, contact_name nvarchar(255), email nvarchar(500),
		last_login datetime, review_role nvarchar(50), review_role_1 nvarchar(50), review_role_2 nvarchar(50),
		is_coding_only bit, is_read_only bit, is_admin bit, [expiry_date] datetime, site_lic_id int, site_lic_name nvarchar(50))

	declare @tb_expiry_dates table
		(contactID int, expiryDate datetime, siteLicID int, siteLicName nvarchar(50))

		
	declare @review_contact_id int
    
    insert into @tb_review_members (contact_id, review_contact_id, contact_name, email)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from Reviewer.dbo.TB_CONTACT c
	right join Reviewer.dbo.TB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
    update @tb_review_members
	set last_login = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where contact_id = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
    
    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members
    
	declare @WORKING_REVIEW_CONTACT_ID int
	declare REVIEW_CONTACT_ID_CURSOR cursor for
	select review_contact_id FROM @tb_review_members
	open REVIEW_CONTACT_ID_CURSOR
	fetch next from REVIEW_CONTACT_ID_CURSOR
	into @WORKING_REVIEW_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		-- see if the person is coding only for this review
		update @tb_review_members 
		set review_role_2 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'Coding only'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		-- see if the person is read only for this review
		update @tb_review_members 
		set review_role_1 = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'ReadOnlyUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from Reviewer.dbo.TB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR

	declare @expiryDate datetime

	declare @WORKING_CONTACT_ID int
	declare CONTACT_ID_CURSOR cursor for
	select contact_id FROM @tb_review_members
	open CONTACT_ID_CURSOR
	fetch next from CONTACT_ID_CURSOR
	into @WORKING_CONTACT_ID
	while @@FETCH_STATUS = 0
	begin 		
		insert into @tb_expiry_dates
		select c.CONTACT_ID, 
			CASE when l.[EXPIRY_DATE] is not null 
			and l.[EXPIRY_DATE] > c.[EXPIRY_DATE]
				then l.[EXPIRY_DATE]
			else c.[EXPIRY_DATE]
			end as 'EXPIRY_DATE', 
		l.SITE_LIC_ID, l.SITE_LIC_NAME
		from Reviewer.dbo.TB_CONTACT c
		left join ReviewerAdmin.dbo.TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join Reviewer.dbo.TB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
		where c.CONTACT_ID = @WORKING_CONTACT_ID
	
		group by c.CONTACT_ID, c.[EXPIRY_DATE], l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME

		FETCH NEXT FROM CONTACT_ID_CURSOR 
		INTO @WORKING_CONTACT_ID
	end

	CLOSE CONTACT_ID_CURSOR
	DEALLOCATE CONTACT_ID_CURSOR

	update t1
	set [expiry_date] = t2.expiryDate,
	site_lic_id = t2.siteLicID,
	site_lic_name = t2.siteLicName
	from @tb_review_members t1 inner join @tb_expiry_dates t2
	on t2.contactID = t1.contact_id
    
    --select * from @tb_review_members
    update @tb_review_members
	set is_coding_only = 1
	where review_role_2 = 'Coding only'
    
    update @tb_review_members
	set is_read_only = 1
	where review_role_1 = 'ReadOnlyUser'
	
	update @tb_review_members
	set is_admin = 1
	where review_role = 'AdminUser'
	
	select * from @tb_review_members
	
	


END
GO

------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ContactReviewsAdmin]    Script Date: 20/03/2020 15:06:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[st_ContactReviewsAdmin] 
(
	@CONTACT_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
    declare @PURCHASED_REVIEWS table ( 
	REVIEW_ID int, 
	REVIEW_NAME nchar(1000),
	DATE_CREATED datetime,
	[EXPIRY_DATE] date,
	MONTHS_CREDIT smallint,
	FUNDER_ID int, 
	LAST_LOGIN datetime,
	SITE_LIC_ID int null,
	SITE_LIC_NAME nvarchar(50) null)

/*
Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT [REVIEW_ID], [REVIEW_NAME], [DATE_CREATED], [EXPIRY_DATE], [MONTHS_CREDIT], [FUNDER_ID]
  FROM [Reviewer].[dbo].[TB_REVIEW] where FUNDER_ID = @CONTACT_ID
  and ((EXPIRY_DATE is null and MONTHS_CREDIT != 0)
		or
		(EXPIRY_DATE is not null))
*/

Insert Into @PURCHASED_REVIEWS (REVIEW_ID, REVIEW_NAME, DATE_CREATED, [EXPIRY_DATE],
				MONTHS_CREDIT, FUNDER_ID)
SELECT distinct r.[REVIEW_ID], r.[REVIEW_NAME], r.[DATE_CREATED], r.[EXPIRY_DATE], 
r.[MONTHS_CREDIT], r.[FUNDER_ID]
  FROM [Reviewer].[dbo].[TB_REVIEW] r
left outer join [Reviewer].[dbo].[TB_REVIEW_CONTACT] r_c
on r.REVIEW_ID = r_c.REVIEW_ID
left outer join [Reviewer].[dbo].[TB_CONTACT_REVIEW_ROLE] c_r_r
on r_c.REVIEW_CONTACT_ID = c_r_r.REVIEW_CONTACT_ID
and ((r_c.CONTACT_ID = @CONTACT_ID and c_r_r.ROLE_NAME = 'AdminUser') 
		or (r.FUNDER_ID = @CONTACT_ID))
where ((r.EXPIRY_DATE is null and r.MONTHS_CREDIT != 0 )
		or (r.EXPIRY_DATE is not null)
		or (r.EXPIRY_DATE is null)) 
		and 
		(r.FUNDER_ID = @CONTACT_ID or (c_r_r.ROLE_NAME is not null and ROLE_NAME = 'AdminUser')) 

update  p_r
set p_r.LAST_LOGIN = 
l_t.CREATED
--max(l_t.CREATED)
from @PURCHASED_REVIEWS p_r, TB_LOGON_TICKET l_t
where l_t.CREATED in
(select max(l_t.CREATED)from TB_LOGON_TICKET l_t
where p_r.FUNDER_ID = l_t.CONTACT_ID
and p_r.REVIEW_ID = l_t.REVIEW_ID)

update @PURCHASED_REVIEWS set SITE_LIC_ID = a.site_lic_id, SITE_LIC_NAME = a.SITE_LIC_NAME
	, [EXPIRY_DATE] = case when L_EXP is not null and [EXPIRY_DATE] > L_EXP  then [EXPIRY_DATE] 
						else L_EXP
					end
from (select ld.SITE_LIC_ID, l.SITE_LIC_NAME, lr.REVIEW_ID as REV_ID , l.[EXPIRY_DATE] as L_EXP
	from TB_SITE_LIC_DETAILS ld inner join Reviewer.dbo.TB_SITE_LIC_REVIEW lr on ld.SITE_LIC_ID = lr.SITE_LIC_ID
	inner join @PURCHASED_REVIEWS pr on pr.REVIEW_ID = lr.REVIEW_ID
	inner join Reviewer.dbo.TB_SITE_LIC l on lr.SITE_LIC_ID = l.SITE_LIC_ID) as a
where a.REV_ID = REVIEW_ID
select * from @PURCHASED_REVIEWS
    
END


GO

------------------------------------

Use ReviewerAdmin
GO

select * from TB_EXTENSION_TYPES
where EXTENSION_TYPE = 'Using credit purchase'
if @@ROWCOUNT = 0
begin
	insert into TB_EXTENSION_TYPES (EXTENSION_TYPE, [DESCRIPTION], APPLIES_TO, [ORDER])
	VALUES ('Using credit purchase', 'The user is applied a credit purchase towards this extension', 111, 11)
end
GO

-------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_PriorityScreeningTurnOnOff]    Script Date: 20/03/2020 15:13:58 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



ALTER procedure [dbo].[st_PriorityScreeningTurnOnOff]
(
	@REVIEW_ID int,
	@FIELD nvarchar(50),
	@SETTING bit
)

As
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here 

	if @FIELD = 'ShowScreening'
	begin
		update Reviewer.dbo.TB_REVIEW
		set SHOW_SCREENING = @SETTING
		where REVIEW_ID = @REVIEW_ID
	end
	
	if @FIELD = 'AllowReviewerTerms'
	begin
		update Reviewer.dbo.TB_REVIEW
		set ALLOW_REVIEWER_TERMS = @SETTING
		where REVIEW_ID = @REVIEW_ID
	end
	
	if @FIELD = 'AllowClusteredSearch'
	begin
		update Reviewer.dbo.TB_REVIEW
		set ALLOW_CLUSTERED_SEARCH = @SETTING
		where REVIEW_ID = @REVIEW_ID
	end

END



GO

--------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewDetailsCochrane]    Script Date: 20/03/2020 15:14:37 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_ReviewDetailsCochrane] 
(
	@REVIEW_ID nvarchar(50)
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here	
	select r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, r.REVIEW_NUMBER, 
	r.DATE_CREATED, r.OLD_REVIEW_GROUP_ID,
		CASE when l.[EXPIRY_DATE] is not null 
		and l.[EXPIRY_DATE] > r.[EXPIRY_DATE]
			then l.[EXPIRY_DATE]
		else r.[EXPIRY_DATE]
		end as 'EXPIRY_DATE', 
	r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.SITE_LIC_ID, l.SITE_LIC_NAME, r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY, r.MAG_ENABLED
	from Reviewer.dbo.TB_REVIEW r
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = r.FUNDER_ID
	left join Reviewer.dbo.TB_SITE_LIC_REVIEW sr on r.REVIEW_ID = sr.REVIEW_ID
	left join Reviewer.dbo.TB_SITE_LIC l on sr.SITE_LIC_ID = l.SITE_LIC_ID
	where r.REVIEW_ID = @REVIEW_ID
	
	group by r.REVIEW_ID, r.old_review_id, r.REVIEW_NAME, 
	r.DATE_CREATED, r.[EXPIRY_DATE],  r.MONTHS_CREDIT, r.FUNDER_ID, c.CONTACT_NAME,
	l.[EXPIRY_DATE], l.SITE_LIC_ID, l.SITE_LIC_NAME, r.REVIEW_NUMBER, r.OLD_REVIEW_GROUP_ID,
	r.SHOW_SCREENING, r.ALLOW_REVIEWER_TERMS, r.ALLOW_CLUSTERED_SEARCH,
	r.ARCHIE_ID, r.ARCHIE_CD, r.IS_CHECKEDOUT_HERE, r.CHECKED_OUT_BY, r.MAG_ENABLED
	order by r.REVIEW_NAME
	

	RETURN

END

GO

---------------------------------------------

