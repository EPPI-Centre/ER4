USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchaseGet]    Script Date: 23/04/2020 14:57:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER   PROCEDURE [dbo].[st_CreditPurchaseGet] 
(
	@CREDIT_PURCHASE_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	/*
	select cp.PURCHASER_CONTACT_ID, cp.DATE_PURCHASED, cp.CREDIT_PURCHASED, 
	     cp.NOTES, c.CONTACT_NAME, c.EMAIL from TB_CREDIT_PURCHASE cp
	inner join Reviewer.dbo.TB_CONTACT c on c.CONTACT_ID = cp.PURCHASER_CONTACT_ID
	where cp.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	*/


	declare @reviewPrice int
	declare @accountPrice int
	declare @forSaleID int 
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Professional') -- account
	set @accountPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)
	set @forSaleID = (select FOR_SALE_ID from TB_FOR_SALE where TYPE_NAME = 'Shareable Review') -- review
	set @reviewPrice = (select PRICE_PER_MONTH from TB_FOR_SALE where FOR_SALE_ID = @forSaleID)


	declare @tv_credit_purchase table (tv_credit_purchase_id int, tv_credit_purchaser_id int, tv_date_purchased date, tv_credit_purchased int,
		tv_credit_remaining int, tv_notes nvarchar(max), tv_contact_name nvarchar(255), tv_email nvarchar(500))
	insert into @tv_credit_purchase (tv_credit_purchase_id, tv_credit_purchaser_id, tv_date_purchased, tv_credit_purchased, tv_notes)
	select CREDIT_PURCHASE_ID, PURCHASER_CONTACT_ID, DATE_PURCHASED, CREDIT_PURCHASED, NOTES from TB_CREDIT_PURCHASE 
	where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID

	declare @tv_credit_history table (tv_credit_extension_id int, tv_type_extended_id int, 
		tv_type_extended_name nvarchar(1000), tv_id_extended int,  
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int)


	declare @totalUsed int = 0
	declare @remainingCredit int = 0
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

	update @tv_credit_purchase set tv_credit_remaining = @remainingCredit
	where tv_credit_purchase_id = @CREDIT_PURCHASE_ID


	update @tv_credit_purchase
	set tv_contact_name = (select CONTACT_NAME from Reviewer.dbo.TB_CONTACT 
	where CONTACT_ID = (select tv_credit_purchaser_id from @tv_credit_purchase))

	update @tv_credit_purchase
	set tv_email = (select EMAIL from Reviewer.dbo.TB_CONTACT 
	where CONTACT_ID = (select tv_credit_purchaser_id from @tv_credit_purchase))


	select * from @tv_credit_purchase

    	
	RETURN
END


GO

