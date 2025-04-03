USE [ReviewerAdmin]
GO

IF COL_LENGTH('dbo.TB_MANAGEMENT_SETTINGS', 'ENABLE_CHAPGPT_ENABLER') IS NULL
BEGIN

BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION

ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS ADD
	ENABLE_CHAPGPT_ENABLER bit NOT NULL DEFAULT(0)

ALTER TABLE dbo.TB_MANAGEMENT_SETTINGS SET (LOCK_ESCALATION = TABLE)

COMMIT
END
GO
--END of CREATE COLUMN Example

---------------------------------------------------------------------

USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_SetCreditPurchaseIDForOpenAIByPurchaserID]    Script Date: 03/03/2025 14:06:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE or ALTER          procedure [dbo].[st_SetCreditPurchaseIDForOpenAIByPurchaserID]
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

	select * from TB_CREDIT_PURCHASE where CREDIT_PURCHASE_ID = @CREDIT_PURCHASE_ID and PURCHASER_CONTACT_ID = @CONTACT_ID
	if @@ROWCOUNT = 0
	begin
		-- this isn't a valied credit purchase ID for this user
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
			
				-- log the entry in TB_SITE_LIC_LOG
				-- find latest package for this site license
				declare @latest_package_id int
				set @latest_package_id = (select top 1 SITE_LIC_DETAILS_ID from TB_SITE_LIC_DETAILS
					where SITE_LIC_ID = @LICENSE_ID order by SITE_LIC_DETAILS_ID desc)

				insert into TB_SITE_LIC_LOG (SITE_LIC_DETAILS_ID, CONTACT_ID, AFFECTED_ID, CHANGE_TYPE, 
					DATE, REASON)
				values (@latest_package_id, @CONTACT_ID, 0, 'add Robot credit', getdate(), 
					'Robot credit from PurchaseID ' + CONVERT(nvarchar(4),@CREDIT_PURCHASE_ID))
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


----------------------------------------------------
