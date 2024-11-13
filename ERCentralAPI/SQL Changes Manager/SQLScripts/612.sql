USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_GetCreditPurchaseIDsForOpenAI]    Script Date: 02/10/2024 13:19:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE or ALTER    procedure [dbo].[st_GetCreditPurchaseIDsForOpenAI]
(
	@REVIEW_ID int,
	@LICENSE_ID int 
)

As


-- if @REVIEW_ID = 0 then it is a site license
-- if @LICENSE_ID = 0 then it is a review


SET NOCOUNT ON

	declare @tv_credit_for_robots table (tv_credit_for_robots_id int, tv_credit_purchase_id int, tv_credit_purchaser_contact_id int, 
			tv_credit_purchaser_contact_name nvarchar(255), tv_remaining nvarchar(50))
		
	
	if @REVIEW_ID = 0   -- its a site license
		begin
			insert into @tv_credit_for_robots (tv_credit_for_robots_id, tv_credit_purchase_id)
			select CREDIT_FOR_ROBOTS_ID, CREDIT_PURCHASE_ID from TB_CREDIT_FOR_ROBOTS where LICENSE_ID = @LICENSE_ID				
		end
		else  -- its a review
		begin
			insert into @tv_credit_for_robots (tv_credit_for_robots_id, tv_credit_purchase_id)
			select CREDIT_FOR_ROBOTS_ID, CREDIT_PURCHASE_ID from TB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID
		end


		update @tv_credit_for_robots
		set tv_credit_purchaser_contact_id = PURCHASER_CONTACT_ID
		from TB_CREDIT_PURCHASE, @tv_credit_for_robots
		where CREDIT_PURCHASE_ID = tv_credit_purchase_id

		update @tv_credit_for_robots
		set tv_credit_purchaser_contact_name = CONTACT_NAME
		from sTB_CONTACT, @tv_credit_for_robots
		where CONTACT_ID = tv_credit_purchaser_contact_id

		
		declare @TMP_REMAINING nvarchar(50)
		-- to call the function I need to use a loop
		declare @WORKING_CREDIT_PURCHASE_ID int
		declare CREDIT_PURCHASE_ID_CURSOR cursor FORWARD_ONLY READ_ONLY for
		select tv_credit_purchase_id FROM @tv_credit_for_robots
		open CREDIT_PURCHASE_ID_CURSOR
		fetch next from CREDIT_PURCHASE_ID_CURSOR
		into @WORKING_CREDIT_PURCHASE_ID
		while @@FETCH_STATUS = 0
		begin
			set @TMP_REMAINING = (select details.tv_credit_remaining from dbo.fn_CreditRemainingDetails(@WORKING_CREDIT_PURCHASE_ID) as details)
			
			update @tv_credit_for_robots
			set tv_remaining = @TMP_REMAINING
			where tv_credit_purchase_id = @WORKING_CREDIT_PURCHASE_ID

			FETCH NEXT FROM CREDIT_PURCHASE_ID_CURSOR 
			INTO @WORKING_CREDIT_PURCHASE_ID
		END

		CLOSE CREDIT_PURCHASE_ID_CURSOR
		DEALLOCATE CREDIT_PURCHASE_ID_CURSOR

		select * from @tv_credit_for_robots

	
SET NOCOUNT OFF



GO


------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_SetCreditPurchaseIDForOpenAI]    Script Date: 03/10/2024 10:14:33 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE or ALTER  procedure [dbo].[st_SetCreditPurchaseIDForOpenAI]
(
	@CREDIT_PURCHASE_ID int,
	@REVIEW_ID int,
	@LICENSE_ID int,
	@RESULT nvarchar(50) output
)

As

-- if @REVIEW_ID = 0 then it is a site license
-- if @LICENSE_ID = 0 then it is a review


SET NOCOUNT ON


if @CREDIT_PURCHASE_ID != 0  -- this is a 'set' in TB_CREDIT_FOR_ROBOTS
begin		
		
	set @RESULT = 'SUCCESS'

	select * from TB_CREDIT_PURCHASE where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
	if @@ROWCOUNT = 0
	begin
		-- this isn't a valied credit purchase ID
		set @RESULT = 'Invalid credit purchase ID'
	end

	if @RESULT = 'SUCCESS'   -- update TB_CREDIT_FOR_ROBOTS
	begin			
		if @REVIEW_ID = 0   -- its a site license
		begin
			if EXISTS (select * from TB_CREDIT_FOR_ROBOTS where LICENSE_ID = @LICENSE_ID and CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID)
			begin
				-- the CREDIT_PURCHASE_ID is already in TB_CREDIT_FOR_ROBOTS for this site license
				set @RESULT = 'Invalid credit purchase ID'
			end
			else
			begin
				insert into TB_CREDIT_FOR_ROBOTS (CREDIT_PURCHASE_ID, LICENSE_ID)
				values (@CREDIT_PURCHASE_ID, @LICENSE_ID)
			end
		end
		else  -- its a review
		begin
			if EXISTS (select * from TB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID and CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID)
			begin
				-- the CREDIT_PURCHASE_ID is already in TB_CREDIT_FOR_ROBOTS for this review
				set @RESULT = 'Invalid credit purchase ID'
			end
			else
			begin
				insert into TB_CREDIT_FOR_ROBOTS (CREDIT_PURCHASE_ID, REVIEW_ID)
				values (@CREDIT_PURCHASE_ID, @REVIEW_ID)
			end
		end
	end
		
end

	

	
SET NOCOUNT OFF



GO


--------------------------------------------------

USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_RemoveCreditPurchaseIDForOpenAI]    Script Date: 03/10/2024 14:20:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE or ALTER  procedure [dbo].[st_RemoveCreditPurchaseIDForOpenAI]
(
	@CREDIT_FOR_ROBOTS_ID int
)

As


SET NOCOUNT ON

	delete from TB_CREDIT_FOR_ROBOTS			
	where CREDIT_FOR_ROBOTS_ID = @CREDIT_FOR_ROBOTS_ID

	
SET NOCOUNT OFF



GO


-----------------------------------------------------------

