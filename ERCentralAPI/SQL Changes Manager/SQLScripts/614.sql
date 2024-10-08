USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_RemoveCreditPurchaseIDForOpenAI]    Script Date: 07/10/2024 14:39:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE or ALTER    procedure [dbo].[st_RemoveCreditPurchaseIDForOpenAI]
(
	@CREDIT_FOR_ROBOTS_ID int,
	@CONTACT_ID int,
	@REVIEW_ID int,
	@LICENSE_ID int
)

As


SET NOCOUNT ON

	declare @credit_purchase_id int
	set @credit_purchase_id = (select CREDIT_PURCHASE_ID from TB_CREDIT_FOR_ROBOTS			
		where CREDIT_FOR_ROBOTS_ID = @CREDIT_FOR_ROBOTS_ID)

	delete from TB_CREDIT_FOR_ROBOTS			
	where CREDIT_FOR_ROBOTS_ID = @CREDIT_FOR_ROBOTS_ID


	if @REVIEW_ID != 0
	begin
		-- log the entry in TB_EXPIRY_EDIT_LOG if it is a review extension
		declare @purchase_id nvarchar(10)
		set @purchase_id = (select STR(@credit_purchase_id))

		declare @expiry_date date
		set @expiry_date = (select EXPIRY_DATE from sTB_REVIEW where REVIEW_ID = @REVIEW_ID)

		insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
			ID_EXTENDED, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
		values (getdate(), 0, @expiry_date, @expiry_date,
			@REVIEW_ID, @CONTACT_ID, 22, 'removed GPT PurchaseID: ' + @purchase_id)
	end


	
SET NOCOUNT OFF



------------------------------------------

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_SetCreditPurchaseIDForOpenAI]    Script Date: 07/10/2024 13:05:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE or ALTER    procedure [dbo].[st_SetCreditPurchaseIDForOpenAI]
(
	@CREDIT_PURCHASE_ID int,
	@REVIEW_ID int,
	@LICENSE_ID int,
	@CONTACT_ID int,
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
				
				declare @purchase_id nvarchar(10)
				set @purchase_id = (select STR(@CREDIT_PURCHASE_ID))

				declare @expiry_date date
				set @expiry_date = (select EXPIRY_DATE from sTB_REVIEW where REVIEW_ID = @REVIEW_ID)

				-- log the entry in TB_EXPIRY_EDIT_LOG
				insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
					ID_EXTENDED, EXTENDED_BY_ID, EXTENSION_TYPE_ID, EXTENSION_NOTES)
				values (getdate(), 0, @expiry_date, @expiry_date,
					@REVIEW_ID, @CONTACT_ID, 22, 'added GPT PurchaseID: ' + @purchase_id)
			end
		end
	end
		
end

	

	
SET NOCOUNT OFF



