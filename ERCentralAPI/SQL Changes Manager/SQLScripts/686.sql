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
