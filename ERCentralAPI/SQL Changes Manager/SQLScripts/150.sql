USE [ReviewerAdmin]
GO
/****** Object:  StoredProcedure [dbo].[st_CheckLinkCheck]    Script Date: 2/4/2020 12:34:42 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[st_CheckLinkCheck]
	-- Add the parameters for the stored procedure here
	 @CID int
	, @UID uniqueidentifier 
	, @RESULT varchar(15) OUTPUT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DECLARE @DateCheck Datetime2(1)
    DECLARE @Type varchar(15)
    Select @DateCheck = DATE_CREATED, @Type = [TYPE] 
		from TB_CHECKLINK where CONTACT_ID = @CID and CHECKLINK_UID = @UID and IS_STALE = 0 and WAS_SUCCESS is null
		--actually, checking IS_STALE = 0 and WAS_SUCCESS is null here makes the first failure quite unspecific...
	if @DateCheck is null OR @@ROWCOUNT != 1 
	begin
		set @RESULT = 'Not found'
		return
	end
	--possible types are: CheckEmail, ResetPw and ActivateGhost
	IF @Type = 'CheckEmail'
	Begin
		if DATEDIFF(D, @DateCheck, GETDATE()) <= 7
		begin
			set @RESULT = @Type
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 1 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		else 
		Begin
			set @RESULT = 'ExpiredCkEmail'
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		return
	END
	if @Type = 'ResetPw'
	Begin
		if DATEDIFF(MINUTE, @DateCheck, GETDATE()) <= 60
		begin
			set @RESULT = @Type
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE() where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		else
		begin
			set @RESULT = 'ExpiredResetPw'
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		return
	END
	if @Type = 'ActivateGhost'
	Begin
		if DATEDIFF(D, @DateCheck, GETDATE()) <= 14
		begin
			set @RESULT = @Type
		end
		else 
		begin
			set @RESULT = 'ExpiredActGhost'
			UPDATE TB_CHECKLINK set IS_STALE = 1, DATE_USED = GETDATE(), WAS_SUCCESS = 0 where CHECKLINK_UID = @UID and @CID = CONTACT_ID
		end
		return
	END
	
END
GO
