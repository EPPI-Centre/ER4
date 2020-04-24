Use ReviewerAdmin
GO

select * from TB_MANAGMENT_EMAILS
where EMAIL_ID = 11
if @@ROWCOUNT = 0
begin
	insert into TB_MANAGMENT_EMAILS (EMAIL_NAME, EMAIL_MESSAGE)
	values ('Outstanding fee', 
	'<p>
	EPPI-Reviewer: Software for research synthesis<br />
	<br />
	Dear FullNameHere,<br />
	<br />
	There is an outstanding fee of ShowFeeHere related to previous EPPI-Reviewer account and/or review extensions and/or EPPI-Reviewer support that have not yet been paid for.</p>
	<p>A bill for these services has been created in the online shop that you can access through the ACCOUNT MANAGER (that can be found on the EPPI-Reviewer gateway <a href="https://eppi.ioe.ac.uk/cms/er4">here</a>).</p>
	<p>If you have any questions about this please contact us at&nbsp;<a href="mailto:EPPIsupport@ioe.ac.uk"><span style="color: #0066cc;">EPPIsupport@ucl.ac.uk</span></a></p>
	<p>&nbsp;</p>
	<p>
	EPPI-Reviewer is developed and maintained by the EPPI-Centre of the UCL Institute of Education at the University of London, UK. To find out more about the work of the EPPI-Centre please visit our website <a href="http://eppi.ioe.ac.uk/"><span style="color: purple;">eppi.ioe.ac.uk</span></a>.<br />
	<br />
	In case this message is unexpected, please don&rsquo;t hesitate to contact our support staff:<br />
	<a href="mailto:EPPIsupport@ioe.ac.uk"><span style="color: #0066cc;">EPPIsupport@ucl.ac.uk</span></a><br />
	<br />
	</p>
	<br />')
end
GO


USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_BillRemoveOutstandingFee]    Script Date: 21/04/2020 10:25:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








-- =============================================
-- Author:		Sergio
-- Create date: <Create Date,,>
-- Description:	adds an existing review to a draft bill need exact review_ID and matching full name.
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_BillRemoveOutstandingFee]
(
	@bill_ID int
)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    declare @FOR_SALE_ID int
	declare @CHK int
	declare @numberMonths int
	 

	select top 1 @FOR_SALE_ID = FOR_SALE_ID from TB_FOR_SALE
	where [TYPE_NAME] = 'Outstanding fee'
	order by LAST_CHANGED desc

	
	-- find the relevant entry in TB_BILL_LINE and remove it
	select * from TB_BILL_LINE
	where BILL_ID = @BILL_ID
	and FOR_SALE_ID = @FOR_SALE_ID
	if @@ROWCOUNT = 1
	begin	
		delete from TB_BILL_LINE
		where BILL_ID = @bill_ID
		and FOR_SALE_ID = @FOR_SALE_ID
	end

END
GO

--------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditPurchasesByPurchaser]    Script Date: 22/04/2020 11:34:07 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO







-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_CreditPurchasesByPurchaser] 
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

		-- where a non-sharable review was extended and didn't have a tv_old_date
		update @tv_credit_history set tv_old_date = tv_date_extended where tv_old_date is null		

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

		delete from @tv_credit_history

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

---------------------------
USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 22/04/2020 11:34:43 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO








-- =============================================
-- Author:		<Author,,Name>
-- ALTER date: <ALTER Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE OR ALTER   PROCEDURE [dbo].[st_CreditHistoryByPurchase] 
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

	-- where a non-sharable review was extended and didn't have a tv_old_date
	update @tv_credit_history set tv_old_date = tv_date_extended where tv_old_date is null		

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

----------------------

