USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_ReturnToCredit]    Script Date: 29/05/2025 08:57:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE OR ALTER     PROCEDURE [dbo].[st_ReturnToCredit] 
	-- Add the parameters for the stored procedure here
	@CREDIT_EXTENSION_ID int,
	@TYPE_TO_CREDIT nvarchar(10),
	@CONTACT_OR_REVIEW_ID int,
	@MONTHS_TO_CREDIT int,
	@RESULT nvarchar(100) output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- we need to edit TB_EXPIRY_EDIT_LOG and sTB_CONTACT with new expiry dates
	declare @expiryEditId int
	declare @creditPurchaseId int
	declare @expiryDateFromTbContact date
	declare @expiryDateFromTbReview date
	declare @expiryDateFromTbExpiryEditLog date
	declare @newExpiryDate date

	set @expiryEditId = (select EXPIRY_EDIT_ID from TB_CREDIT_EXTENSIONS
		where CREDIT_EXTENSION_ID = @CREDIT_EXTENSION_ID)
	set @creditPurchaseId = (select CREDIT_PURCHASE_ID from TB_CREDIT_EXTENSIONS
		where CREDIT_EXTENSION_ID = @CREDIT_EXTENSION_ID)

	if @TYPE_TO_CREDIT = 'Account'
	begin
		set @expiryDateFromTbContact = (select EXPIRY_DATE from sTB_CONTACT
			where CONTACT_ID = @CONTACT_OR_REVIEW_ID)
		set @expiryDateFromTbExpiryEditLog = (select NEW_EXPIRY_DATE from TB_EXPIRY_EDIT_LOG
			where EXPIRY_EDIT_ID = @expiryEditId)
		if @expiryDateFromTbContact = @expiryDateFromTbExpiryEditLog
		begin
			set @newExpiryDate = (SELECT DATEADD (month,-@MONTHS_TO_CREDIT,@expiryDateFromTbContact))
			-- edit TB_EXPIRY_EDIT_LOG			
			update TB_EXPIRY_EDIT_LOG
			set NEW_EXPIRY_DATE = @newExpiryDate
			where EXPIRY_EDIT_ID = @expiryEditId
			-- edit TB_CONTACT
			update sTB_CONTACT
			set EXPIRY_DATE = @newExpiryDate
			where CONTACT_ID = @CONTACT_OR_REVIEW_ID

			set @RESULT = 'Success'
		end
		else
		begin
			-- there is a mismatch between expiry dates so return a mismatch error
			set @RESULT = 'There was a expiry date mismatch. Please contact eppi-support'
		end
	end
	else
	begin
		-- it's a review
		set @expiryDateFromTbReview = (select EXPIRY_DATE from sTB_REVIEW
			where REVIEW_ID = @CONTACT_OR_REVIEW_ID)
		set @expiryDateFromTbExpiryEditLog = (select NEW_EXPIRY_DATE from TB_EXPIRY_EDIT_LOG
			where EXPIRY_EDIT_ID = @expiryEditId)

		if @expiryDateFromTbReview = @expiryDateFromTbExpiryEditLog
		begin
			set @newExpiryDate = (SELECT DATEADD (month,-@MONTHS_TO_CREDIT,@expiryDateFromTbReview))
			-- edit TB_EXPIRY_EDIT_LOG			
			update TB_EXPIRY_EDIT_LOG
			set NEW_EXPIRY_DATE = @newExpiryDate
			where EXPIRY_EDIT_ID = @expiryEditId
			-- edit TB_CONTACT
			update sTB_REVIEW
			set EXPIRY_DATE = @newExpiryDate
			where REVIEW_ID = @CONTACT_OR_REVIEW_ID

			set @RESULT = 'Success'
		end
		else
		begin
			-- there is a mismatch between expiry dates so return a mismatch error
			set @RESULT = 'There was a expiry date mismatch. Please contact eppi-support'
		end
	end



END

GO

