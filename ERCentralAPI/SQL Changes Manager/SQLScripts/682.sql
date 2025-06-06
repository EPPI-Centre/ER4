USE [ReviewerAdmin]
GO

/****** Object:  StoredProcedure [dbo].[st_TransferCreditFromGhostAccount]    Script Date: 02/06/2025 15:05:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





ALTER     procedure st_TransferCreditFromGhostAccount
(
	@SOURCE_CONTACT_ID int,
    @DESTINATION_CONTACT_ID int,
    @MONTHS_CREDIT int,
	@EXTENDED_BY_ID int
)

As

SET NOCOUNT ON

	declare @existingExpiryDate datetime
	declare @newExpiryDate datetime
	declare @contactIdAsString nvarchar(20)
	set @contactIdAsString = CONVERT(NVARCHAR(20), @DESTINATION_CONTACT_ID)


	--  update TB_CONTACT for the Ghost account
	update sTB_CONTACT
	set CONTACT_NAME = 'Transferred to ID:' + @contactIdAsString, 
	MONTHS_CREDIT = @MONTHS_CREDIT * -1
	where CONTACT_ID = @SOURCE_CONTACT_ID

	--  update TB_CONTACT for the destination account
	set @existingExpiryDate = (select EXPIRY_DATE from sTB_CONTACT where CONTACT_ID = @DESTINATION_CONTACT_ID)
	IF @existingExpiryDate < GETDATE() set @existingExpiryDate = GETDATE()
	set @newExpiryDate = DATEADD(Month, @MONTHS_CREDIT, @existingExpiryDate)
	update sTB_CONTACT
	set EXPIRY_DATE = @newExpiryDate
	where CONTACT_ID = @DESTINATION_CONTACT_ID

	-- add a new line to TB_EXPIRY_EDIT_LOG
	set @contactIdAsString = CONVERT(NVARCHAR(20), @SOURCE_CONTACT_ID)
	insert into TB_EXPIRY_EDIT_LOG (DATE_OF_EDIT, TYPE_EXTENDED, ID_EXTENDED, OLD_EXPIRY_DATE, NEW_EXPIRY_DATE, 
		EXTENDED_BY_ID,EXTENSION_TYPE_ID, EXTENSION_NOTES)
	values (GETDATE(), 1, @DESTINATION_CONTACT_ID, @existingExpiryDate, @newExpiryDate, @EXTENDED_BY_ID, 22, 'Transfer of credit from ghost account ' + @contactIdAsString)



	

SET NOCOUNT OFF



GO


