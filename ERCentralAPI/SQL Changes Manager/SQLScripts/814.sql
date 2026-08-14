USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ApplyCreditToReview]    Script Date: 14/08/2026 10:17:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER     procedure [dbo].[st_ApplyCreditToReview]
(
	@CREDIT_PURCHASE_ID int,
	@REVIEW_ID int,
	@MONTHS_EXTENDED int,
	@PERSON_EXTENDING_CONTACT_ID int,
	@res int output 

)

As

SET NOCOUNT ON

	set @res = 0

	-- added JB 13/08/2026 for bug fix
	-- we need to check if there is sufficient credit to make this extension
	-- we already have a function (fn_CreditRemainingDetails) that calculates the remaining credit and
	-- although it is overkill because it finds all remaining credits but we can take what we need from the results
	declare @remaining_credit int
		declare @tv_credit_purchases table (tv_credit_purchase_id int, tv_date_purchased date, tb_credit_purchased int,
		tv_credit_remaining int)
	insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased, tv_credit_remaining)
	  SELECT tv_credit_purchase_id, tv_date_purchased, tv_credit_purchased, tv_credit_remaining 
	  from TB_CREDIT_PURCHASE cp
		Cross apply dbo.fn_CreditRemainingDetails(cp.CREDIT_PURCHASE_ID)  as details 
		where details.tv_credit_purchase_id = cp.CREDIT_PURCHASE_ID and cp.PURCHASER_CONTACT_ID = @PERSON_EXTENDING_CONTACT_ID

	set @remaining_credit = (select tv_credit_remaining from @tv_credit_purchases where tv_credit_purchase_id = @CREDIT_PURCHASE_ID)	

	declare @reviewFee int = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_Id = 4)
	declare @amountToApply int = @MONTHS_EXTENDED * @reviewFee
	if @remaining_credit >= @amountToApply
	begin
		set @res = 1
		-- extend the review by the selected number of months
	
		-- is the review activated
		declare @monthsCredit int
		declare @oldExpiryDate date
		declare @newExpiryDate date
		declare @newExpiryEditID int

		set @monthsCredit = (select MONTHS_CREDIT from sTB_REVIEW
		where REVIEW_ID = @REVIEW_ID)

		declare @extensionTypeID int = (select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Using credit purchase')

		if @monthsCredit > 0
		begin
			-- this is an unactivated review so increase the months of credit
			set @monthsCredit = @monthsCredit + @MONTHS_EXTENDED
			update sTB_REVIEW set MONTHS_CREDIT = @monthsCredit
			where REVIEW_ID = @REVIEW_ID
			set @oldExpiryDate = null
			set @newExpiryDate = null
		end
		else
		begin
			set @oldExpiryDate = (select EXPIRY_DATE from sTB_REVIEW
				where REVIEW_ID = @REVIEW_ID)
			if @oldExpiryDate is null
			begin
				-- this is a non-shareable review so make it shareable 
				update sTB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
				where REVIEW_ID = @REVIEW_ID
				set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
			end
			else
			begin
				-- this is a shareable review so extend the expiry date
				if @oldExpiryDate <= getdate()
				begin
					-- it expired so add from today
					update sTB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
					where REVIEW_ID = @REVIEW_ID
					set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
				end
				else
				begin
					-- not yet expired so add from old expiry date
					update sTB_REVIEW set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
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
	
	end


SET NOCOUNT OFF

GO

------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ApplyCreditToAccount]    Script Date: 14/08/2026 10:18:09 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER     procedure [dbo].[st_ApplyCreditToAccount]
(
	@CREDIT_PURCHASE_ID int,
	@CONTACT_ID int,
	@MONTHS_EXTENDED int,
	@PERSON_EXTENDING_CONTACT_ID int,
	@res int output
)

As

SET NOCOUNT ON

	-- extend the review by the selected number of months
	
	set @res = 0

	-- added JB 13/08/2026 for bug fix
	-- we need to check if there is sufficient credit to make this extension
	-- we already have a function (fn_CreditRemainingDetails) that calculates the remaining credit and
	-- although it is overkill because it finds all remaining credits but we can take what we need from the results
	declare @remaining_credit int
		declare @tv_credit_purchases table (tv_credit_purchase_id int, tv_date_purchased date, tb_credit_purchased int,
		tv_credit_remaining int)
	insert into @tv_credit_purchases (tv_credit_purchase_id, tv_date_purchased, tb_credit_purchased, tv_credit_remaining)
	  SELECT tv_credit_purchase_id, tv_date_purchased, tv_credit_purchased, tv_credit_remaining 
	  from TB_CREDIT_PURCHASE cp
		Cross apply dbo.fn_CreditRemainingDetails(cp.CREDIT_PURCHASE_ID)  as details 
		where details.tv_credit_purchase_id = cp.CREDIT_PURCHASE_ID and cp.PURCHASER_CONTACT_ID = @PERSON_EXTENDING_CONTACT_ID

	set @remaining_credit = (select tv_credit_remaining from @tv_credit_purchases where tv_credit_purchase_id = @CREDIT_PURCHASE_ID)	

	declare @accountFee int = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_Id = 3)
	declare @amountToApply int = @MONTHS_EXTENDED * @accountFee
	if @remaining_credit >= @amountToApply
	begin
		set @res = 1

		-- is the review activated
		declare @monthsCredit int
		declare @oldExpiryDate date
		declare @newExpiryDate date
		declare @newExpiryEditID int

		set @monthsCredit = (select MONTHS_CREDIT from sTB_CONTACT
		where CONTACT_ID = @CONTACT_ID)

		declare @extensionTypeID int = (select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Using credit purchase')

		if @monthsCredit > 0
		begin
			-- this is an unactivated user account so increase the months of credit
			set @monthsCredit = @monthsCredit + @MONTHS_EXTENDED
			update sTB_CONTACT set MONTHS_CREDIT = @monthsCredit
			where CONTACT_ID = @CONTACT_ID
			set @oldExpiryDate = null
			set @newExpiryDate = null
		end
		else
		begin
			set @oldExpiryDate = (select EXPIRY_DATE from sTB_CONTACT
				where CONTACT_ID = @CONTACT_ID)
			if @oldExpiryDate <= getdate()
			begin
				-- it expired so add from today
				update sTB_CONTACT set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
					where CONTACT_ID = @CONTACT_ID
				set @newExpiryDate = DATEADD(MONTH,@MONTHS_EXTENDED,GETDATE())
			end
			else
			begin
				-- not yet expired so add from old expiry date
				update sTB_CONTACT set EXPIRY_DATE = DATEADD(MONTH,@MONTHS_EXTENDED,@oldExpiryDate)
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
		
	end

SET NOCOUNT OFF

GO

-------------------------------------