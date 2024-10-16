USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewMembers]    Script Date: 16/10/2024 10:24:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_ReviewMembers] 
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
	from sTB_CONTACT c
	right join sTB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
	/* bug
    update @tb_review_members
	set last_login = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where contact_id = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
	*/
    
    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members

	declare @tmp_contact_id int 
    
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
		(select CRR.ROLE_NAME from sTB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'Coding only'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		-- see if the person is read only for this review
		update @tb_review_members 
		set review_role_1 = 
		(select CRR.ROLE_NAME from sTB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'ReadOnlyUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from sTB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		set @tmp_contact_id =
		(select CONTACT_ID from sTB_REVIEW_CONTACT
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID)

		update @tb_review_members
		set last_login = 
		(select max(lt.CREATED) from TB_LOGON_TICKET lt
		where contact_id = @tmp_contact_id
		and lt.REVIEW_ID = @REVIEW_ID)
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
		from sTB_CONTACT c
		left join TB_LOGON_TICKET lt
		on c.CONTACT_ID = lt.CONTACT_ID
		left join sTB_SITE_LIC_CONTACT sc on c.CONTACT_ID = sc.CONTACT_ID
		left join sTB_SITE_LIC l on sc.SITE_LIC_ID = l.SITE_LIC_ID
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
	where contact_name != 'OpenAI GPT4'
	
	
END
GO

------------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReviewMembers_2]    Script Date: 16/10/2024 11:07:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jeff>
-- ALTER date: <24/03/2010>
-- Description:	<gets the review data based on contact>
-- =============================================
CREATE OR ALTER     PROCEDURE [dbo].[st_ReviewMembers_2] 
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
		is_coding_only bit, is_read_only bit, is_admin bit)
		
	declare @review_contact_id int
    
    insert into @tb_review_members (contact_id, review_contact_id, contact_name, email)
	select c.CONTACT_ID, rc.REVIEW_CONTACT_ID, c.CONTACT_NAME, c.EMAIL
	from sTB_CONTACT c
	right join sTB_REVIEW_CONTACT rc
	on c.CONTACT_ID = rc.CONTACT_ID
	where rc.REVIEW_ID = @REVIEW_ID
	order by c.CONTACT_NAME
    
    --select * from @tb_review_members
    
	/* bug
    update @tb_review_members
	set last_login = 
	(select max(lt.CREATED) as LAST_LOGIN from TB_LOGON_TICKET lt
	where contact_id = lt.CONTACT_ID
	and lt.REVIEW_ID = @REVIEW_ID)
    */

    update @tb_review_members
	set is_coding_only = 0
    
    --select * from @tb_review_members

	declare @tmp_contact_id int 
    
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
		(select CRR.ROLE_NAME from sTB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'Coding only'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		-- see if the person is read only for this review
		update @tb_review_members 
		set review_role_1 = 
		(select CRR.ROLE_NAME from sTB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'ReadOnlyUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID
		
		update @tb_review_members 
		set review_role = 
		(select CRR.ROLE_NAME from sTB_CONTACT_REVIEW_ROLE CRR
		where CRR.REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID
		and CRR.ROLE_NAME = 'AdminUser'
		) 
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		set @tmp_contact_id =
		(select CONTACT_ID from sTB_REVIEW_CONTACT
		where REVIEW_CONTACT_ID = @WORKING_REVIEW_CONTACT_ID)

		update @tb_review_members
		set last_login = 
		(select max(lt.CREATED) from TB_LOGON_TICKET lt
		where contact_id = @tmp_contact_id
		and lt.REVIEW_ID = @REVIEW_ID)
		where review_contact_id = @WORKING_REVIEW_CONTACT_ID

		FETCH NEXT FROM REVIEW_CONTACT_ID_CURSOR 
		INTO @WORKING_REVIEW_CONTACT_ID
	END

	CLOSE REVIEW_CONTACT_ID_CURSOR
	DEALLOCATE REVIEW_CONTACT_ID_CURSOR
    
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
	where contact_name != 'OpenAI GPT4'

END

GO




------------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_TransferCredit]    Script Date: 16/10/2024 10:19:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE OR ALTER   procedure [dbo].[st_TransferCredit]
(
	@SOURCE_PURCHASE_ID int,
    @DESTINATION_PURCHASE_ID int,
    @EXTENSION_NOTES nvarchar(max),
    @EXTENDED_BY int
)

As

SET NOCOUNT ON

	declare @NEW_ROW_ID int

	--  update TB_EXPIRY_EDIT_LOG
	insert into TB_EXPIRY_EDIT_LOG
	(DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, NEW_EXPIRY_DATE,
	OLD_EXPIRY_DATE, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 3, 0, GETDATE(), GETDATE(),
	@EXTENDED_BY, 22, @EXTENSION_NOTES)
	set @NEW_ROW_ID = @@IDENTITY
			
	--  update TB_CREDIT_EXTENSIONS for source
	insert into TB_CREDIT_EXTENSIONS
	(CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
	values (@SOURCE_PURCHASE_ID, @NEW_ROW_ID)

	--  update TB_CREDIT_EXTENSIONS for destination
	insert into TB_CREDIT_EXTENSIONS
	(CREDIT_PURCHASE_ID, EXPIRY_EDIT_ID)
	values (@DESTINATION_PURCHASE_ID, @NEW_ROW_ID)
	

SET NOCOUNT OFF



GO

------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  UserDefinedFunction [dbo].[fn_CreditRemainingDetails]    Script Date: 16/10/2024 10:21:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   FUNCTION [dbo].[fn_CreditRemainingDetails]
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
		tv_date_extended date, tv_new_date date, tv_old_date date, tv_true_old_date date, tv_months_extended int, tv_cost int,
		tv_log_notes nvarchar(max), tv_source int, tv_destination int, tv_amount_transferred int, tv_credit_purchase_id int)


	declare @totalUsed int = 0
	declare @remainingCredit float = 0.0
	declare @creditPurchased int = 0


	set @creditPurchased = (select tv_credit_purchased from @tv_credit_purchase
		where tv_credit_purchase_id = @CREDIT_PURCHASE_ID)
		
	insert into @tv_credit_history (tv_credit_extension_id, tv_type_extended_id, tv_id_extended, 
		tv_date_extended, tv_new_date, tv_old_date, tv_log_notes, tv_credit_purchase_id)
	select ce.CREDIT_EXTENSION_ID, eel.TYPE_EXTENDED, eel.ID_EXTENDED, eel.DATE_OF_EDIT, eel.NEW_EXPIRY_DATE, 
		eel.OLD_EXPIRY_DATE, eel.EXTENSION_NOTES, ce.CREDIT_PURCHASE_ID from TB_CREDIT_EXTENSIONS ce
	inner join TB_EXPIRY_EDIT_LOG eel on eel.EXPIRY_EDIT_ID = ce.EXPIRY_EDIT_ID
	where ce.CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	order by CREDIT_EXTENSION_ID

	-- reviews or accounts
	update @tv_credit_history set tv_type_extended_name = 'Review' where tv_type_extended_id = 0
	update @tv_credit_history set tv_type_extended_name = 'Account' where tv_type_extended_id = 1
	update @tv_credit_history set tv_type_extended_name = 'Transfer' where tv_type_extended_id = 3

	-- get true old_date (i.e. whether the account/review was expired when it was extended)
	update @tv_credit_history set tv_true_old_date = tv_old_date
	update @tv_credit_history set tv_true_old_date = tv_date_extended where tv_date_extended > tv_old_date OR tv_old_date is null

	-- months extended
	update @tv_credit_history set tv_months_extended = (select DATEDIFF(MONTH, tv_true_old_date, tv_new_date))

	-- extract transfer data
	-- need to parse out the data from notes 000000130000001400000020 (S,D,A)
	update @tv_credit_history set tv_source = (SELECT SUBSTRING(tv_log_notes, 1, 8) where tv_type_extended_name = 'Transfer')
	update @tv_credit_history set tv_destination = (SELECT SUBSTRING(tv_log_notes, 9, 8) where tv_type_extended_name = 'Transfer')
	update @tv_credit_history set tv_amount_transferred = (SELECT SUBSTRING(tv_log_notes, 17, 8) where tv_type_extended_name = 'Transfer')


	-- add the total cost
	update @tv_credit_history set tv_cost = tv_months_extended * @accountPrice
	where tv_type_extended_name = 'Account'
	update @tv_credit_history set tv_cost = tv_months_extended * @reviewPrice
	where tv_type_extended_name = 'Review' 
	update @tv_credit_history set tv_cost = tv_amount_transferred 
	where tv_type_extended_name = 'Transfer' and tv_credit_purchase_id = tv_source
	update @tv_credit_history set tv_cost = -1 * tv_amount_transferred 
	where tv_type_extended_name = 'Transfer' and tv_credit_purchase_id = tv_destination

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

-------------------------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_CreditHistoryByPurchase]    Script Date: 16/10/2024 10:23:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER       PROCEDURE [dbo].[st_CreditHistoryByPurchase] 
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
		tv_log_notes nvarchar(max))

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

	select * from @tv_credit_history order by tv_date_extended
       
	END

GO

----------------------------------------------------
