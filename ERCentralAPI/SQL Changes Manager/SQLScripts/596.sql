USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_GetSetCreditPurchaseIDForOpenAI]    Script Date: 09/07/2024 12:51:33 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE or ALTER   procedure [dbo].[st_GetSetCreditPurchaseIDForOpenAI]
(
	@CREDIT_PURCHASE_ID int,
	@REVIEW_ID int,
	@LICENSE_ID int,
	@RESULT nvarchar(50) output, 
	@CREDIT_PURCHASE_ID_FOUND int = 0 output,
	@PURCHASER_CONTACT_ID int = 0 output, 
	@PURCHASER_CONTACT_NAME nvarchar(255) = '' output,
	@REMAINING nvarchar(50) = '0' output 
)

As

-- if @CREDIT_PURCHASE_ID = 0 then it is a 'get'; else it is a 'set'
-- if CREDIT_PURCHASE_ID = -1 then it is clearing from TB_CREDIT_FOR_ROBOTS
-- if @REVIEW_ID = 0 then it is a site license
-- if @LICENSE_ID = 0 then it is a review


SET NOCOUNT ON
declare @tmp_review_id int
declare @tmp_license_id int
declare @tmp_credit_for_robots_id int



	if @CREDIT_PURCHASE_ID != 0  -- this is a 'set' in TB_CREDIT_FOR_ROBOTS
	begin		
		if @CREDIT_PURCHASE_ID = -1  -- this is clearing the value in TB_CREDIT_FOR_ROBOTS
		begin
			-- we also need to know if there are values in both the site license field and the review id field
			if @REVIEW_ID = 0   -- the call is from the site license page
			begin
				set @tmp_review_id = (select REVIEW_ID from TB_CREDIT_FOR_ROBOTS where LICENSE_ID = @LICENSE_ID) 
				set @tmp_credit_for_robots_id = (select CREDIT_FOR_ROBOTS_ID from TB_CREDIT_FOR_ROBOTS where LICENSE_ID = @LICENSE_ID)
				
				if (@tmp_review_id = '') or (@tmp_review_id is null) -- remove the entire row 
				begin
					delete from TB_CREDIT_FOR_ROBOTS			
					where CREDIT_FOR_ROBOTS_ID = @tmp_credit_for_robots_id
					set @RESULT = 'SUCCESS'
				end
				else  -- just clear the license field
				begin
					update TB_CREDIT_FOR_ROBOTS
					set LICENSE_ID = null 
					where CREDIT_FOR_ROBOTS_ID = @tmp_credit_for_robots_id
					set @RESULT = 'SUCCESS'
					
				end
				
			end

			else -- the call is from the review details page
			
			begin
				set @tmp_license_id = (select LICENSE_ID from TB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID)
				set @tmp_credit_for_robots_id = (select CREDIT_FOR_ROBOTS_ID from TB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID)

				if ((@tmp_license_id = '') or (@tmp_license_id is null)) -- remove the entire row 
				begin
					delete from TB_CREDIT_FOR_ROBOTS			
					where CREDIT_FOR_ROBOTS_ID = @tmp_credit_for_robots_id
					set @RESULT = 'SUCCESS'
				end
				else  -- just clear the review field
				begin
					update TB_CREDIT_FOR_ROBOTS
					set REVIEW_ID = null 
					where CREDIT_FOR_ROBOTS_ID = @tmp_credit_for_robots_id
					set @RESULT = 'SUCCESS'			
				end

			end
		end

		else  -- we are seting a value
		
		begin
			
			select * from TB_CREDIT_PURCHASE where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
			if @@ROWCOUNT = 1 
			begin
				-- it is a valid @CREDIT_PURCHASE_ID value so get the associated data
				set @PURCHASER_CONTACT_ID = (select @PURCHASER_CONTACT_ID from TB_CREDIT_PURCHASE 
				where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID)
		
				set @REMAINING = (select details.tv_credit_remaining from dbo.fn_CreditRemainingDetails(@CREDIT_PURCHASE_ID) as details)

				set @RESULT = 'SUCCESS'
			end
			else
			begin
				-- this isn't a valied credit purchase ID
				set @RESULT = 'Invalid credit purchase ID'
				set @PURCHASER_CONTACT_ID = 0
				set @REMAINING = 'N/A'
			end

			if @RESULT = 'SUCCESS'   -- update TB_CREDIT_FOR_ROBOTS
			begin			
				if @REVIEW_ID = 0   -- its a site license
				begin
					if EXISTS (select * from TB_CREDIT_FOR_ROBOTS where LICENSE_ID = @LICENSE_ID)
					begin
						update TB_CREDIT_FOR_ROBOTS
						set CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
						where REVIEW_ID = @REVIEW_ID
					end
					else
					begin
					   insert into TB_CREDIT_FOR_ROBOTS (CREDIT_PURCHASE_ID, LICENSE_ID)
					   values (@CREDIT_PURCHASE_ID, @LICENSE_ID)
					end
				end
				else  -- its a review
				begin
					if EXISTS (select * from TB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID)
					begin
						update TB_CREDIT_FOR_ROBOTS
						set CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID
						where REVIEW_ID = @REVIEW_ID
					end
					else
					begin
					   insert into TB_CREDIT_FOR_ROBOTS (CREDIT_PURCHASE_ID, REVIEW_ID)
					   values (@CREDIT_PURCHASE_ID, @REVIEW_ID)
					end
				end
			end
		end
	end

	else -- if @CREDIT_PURCHASE_ID = 0 then this is a 'get' from TB_CREDIT_FOR_ROBOTS 
	
	begin	
		if @REVIEW_ID = 0   -- its a site license
			begin
				set @CREDIT_PURCHASE_ID_FOUND = (select CREDIT_PURCHASE_ID from TB_CREDIT_FOR_ROBOTS where LICENSE_ID = @LICENSE_ID)				
			end
			else  -- its a review
			begin
				set @CREDIT_PURCHASE_ID_FOUND = (select CREDIT_PURCHASE_ID from TB_CREDIT_FOR_ROBOTS where REVIEW_ID = @REVIEW_ID)
			end

			set @REMAINING = (select details.tv_credit_remaining from dbo.fn_CreditRemainingDetails(@CREDIT_PURCHASE_ID_FOUND) as details)
			set @PURCHASER_CONTACT_ID = (select PURCHASER_CONTACT_ID from TB_CREDIT_PURCHASE where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID_FOUND)
			set @PURCHASER_CONTACT_NAME = (select CONTACT_NAME from sTB_CONTACT where CONTACT_ID = @PURCHASER_CONTACT_ID)
			set @RESULT = 'SUCCESS'
	end


	
SET NOCOUNT OFF


