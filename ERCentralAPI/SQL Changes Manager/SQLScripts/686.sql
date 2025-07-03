USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 28/06/2025 10:08:15 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER           PROCEDURE [dbo].[st_CreditHistoryByPurchase] 
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
		tv_date_extended datetime, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost float,
		tv_log_notes nvarchar(max), tv_tb_contact_or_tb_review_expiry_date datetime)

	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_type_extended_name, tv_id_extended, 
		tv_date_extended, tv_new_date, tv_old_date, tv_cost, tv_name, tv_log_notes)
	Select * from (
		select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, '' as type_extended_name, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
			eel.OLD_EXPIRY_DATE, 0 as tv_cost , '' as tv_name, eel.EXTENSION_NOTES
		from TB_CREDIT_EXTENSIONS ce
		inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
		where ce.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
		--order by CREDIT_EXTENSION_ID
		UNION 
		select null, -1, c.CONTACT_NAME as type_extended_name, l.ROBOT_API_CALL_ID, l.DATE_CREATED, l.DATE_CREATED, 
		l.DATE_CREATED, l.COST as tv_cost, c.CONTACT_NAME as tv_name, ''
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
	update @tv_credit_history set tv_type_extended_name = 'Credit transfer' where tv_type_extended_id = 3

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

	-- transfer details and deal with it in code
	update @tv_credit_history
	set tv_name = tv_log_notes where tv_type_extended_name = 'Credit transfer'

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

	-- add the expiry date from either TB_CONTACT or TB_REVIEW
	update t1
	set tv_tb_contact_or_tb_review_expiry_date = t2.EXPIRY_DATE
	from @tv_credit_history t1 inner join sTB_CONTACT t2
	on t2.CONTACT_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Account'

	update t1
	set tv_tb_contact_or_tb_review_expiry_date = t2.EXPIRY_DATE
	from @tv_credit_history t1 inner join sTB_REVIEW t2
	on t2.REVIEW_ID = t1.tv_id_extended
	where t1.tv_type_extended_name = 'Review'

	select * from @tv_credit_history order by tv_name, tv_date_extended
       
	END

GO


--------------------------------------------------
USE [ReviewerAdmin]
GO

select * from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Returning unused assigned credit'
if @@rowcount = 0
begin
	-- add a new row to TB_EXTENSION_TYPES
	insert into TB_EXTENSION_TYPES (EXTENSION_TYPE, DESCRIPTION, APPLIES_TO, [ORDER])
	values ('Returning unused assigned credit', 'Returning unused assigned credit back to credit purchase', '111', 14)
end

-----------------------------------------------------


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReturnToCredit]    Script Date: 30/06/2025 09:37:21 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE OR ALTER       PROCEDURE [dbo].[st_ReturnToCredit] 
	-- Add the parameters for the stored procedure here
	@CREDIT_EXTENSION_ID int,
	@TYPE_TO_CREDIT nvarchar(10),
	@CONTACT_OR_REVIEW_ID int,
	@MONTHS_TO_CREDIT int,
	@CREDIT_PURCHASE_ID int,
	@RESULT nvarchar(100) output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- we need to edit TB_EXPIRY_EDIT_LOG and sTB_CONTACT with new expiry dates
	declare @expiryEditId int
	--declare @creditPurchaseId int
	declare @expiryDateFromTbContact date
	declare @expiryDateFromTbReview date
	declare @expiryDateFromTbExpiryEditLog date
	declare @newExpiryDate date
	declare @newExpiryEditId int
	declare @extendedByID int
	declare @ExtensionTypeId int = (Select EXTENSION_TYPE_ID from TB_EXTENSION_TYPES where EXTENSION_TYPE = 'Returning unused assigned credit')
	--set @expiryEditId = (select EXPIRY_EDIT_ID from TB_CREDIT_EXTENSIONS
	--	where CREDIT_EXTENSION_ID = @CREDIT_EXTENSION_ID)
	--set @creditPurchaseId = (select CREDIT_PURCHASE_ID from TB_CREDIT_EXTENSIONS
	--	where CREDIT_EXTENSION_ID = @CREDIT_EXTENSION_ID)
	set @extendedByID = (select PURCHASER_CONTACT_ID from TB_CREDIT_PURCHASE
		where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID)


	if @TYPE_TO_CREDIT = 'Account'
	begin
		set @expiryDateFromTbContact = (select EXPIRY_DATE from sTB_CONTACT
			where CONTACT_ID = @CONTACT_OR_REVIEW_ID)
		set @expiryDateFromTbExpiryEditLog = (select NEW_EXPIRY_DATE from TB_EXPIRY_EDIT_LOG
			where EXPIRY_EDIT_ID = @expiryEditId)

		set @newExpiryDate = (SELECT DATEADD (month,-@MONTHS_TO_CREDIT,@expiryDateFromTbContact))
		
		
		-- add a line to TB_EXPIRY_EDIT_LOG and TB_CREDIT_EXTENSIONS
		insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, 
			NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
		values (getdate(), 1, @CONTACT_OR_REVIEW_ID, @expiryDateFromTbContact, 
			@newExpiryDate, @extendedByID, @ExtensionTypeId, 'Returning unused assigned credit')
		set @newExpiryEditId = @@IDENTITY

		insert into TB_CREDIT_EXTENSIONS (CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
		values (@CREDIT_PURCHASE_ID, @newExpiryEditId)


		/*
		-- edit TB_EXPIRY_EDIT_LOG			
		update TB_EXPIRY_EDIT_LOG
		set NEW_EXPIRY_DATE = @newExpiryDate
		where EXPIRY_EDIT_ID = @expiryEditId
		*/

		-- edit TB_CONTACT
		update sTB_CONTACT
		set EXPIRY_DATE = @newExpiryDate
		where CONTACT_ID = @CONTACT_OR_REVIEW_ID
		

		set @RESULT = 'Success'

	end
	else
	begin
		-- it's a review
		set @expiryDateFromTbReview = (select EXPIRY_DATE from sTB_REVIEW
			where REVIEW_ID = @CONTACT_OR_REVIEW_ID)
		set @expiryDateFromTbExpiryEditLog = (select NEW_EXPIRY_DATE from TB_EXPIRY_EDIT_LOG
			where EXPIRY_EDIT_ID = @expiryEditId)

		set @newExpiryDate = (SELECT DATEADD (month,-@MONTHS_TO_CREDIT,@expiryDateFromTbReview))

		-- add a line to TB_EXPIRY_EDIT_LOG and TB_CREDIT_EXTENSIONS
		insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, 
			NEW_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
		values (getdate(), 0, @CONTACT_OR_REVIEW_ID, @expiryDateFromTbReview, 
			@newExpiryDate, @extendedByID, @ExtensionTypeId, 'Returning unused assigned credit')
		set @newExpiryEditId = @@IDENTITY

		insert into TB_CREDIT_EXTENSIONS (CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
		values (@CREDIT_PURCHASE_ID, @newExpiryEditId)
		
		/*
		-- edit TB_EXPIRY_EDIT_LOG			
		update TB_EXPIRY_EDIT_LOG
		set NEW_EXPIRY_DATE = @newExpiryDate
		where EXPIRY_EDIT_ID = @expiryEditId
		*/

		-- edit TB_REVIEW
		update sTB_REVIEW
		set EXPIRY_DATE = @newExpiryDate
		where REVIEW_ID = @CONTACT_OR_REVIEW_ID
		

		set @RESULT = 'Success'
		

	end


END

GO

-----------------------------------------------




