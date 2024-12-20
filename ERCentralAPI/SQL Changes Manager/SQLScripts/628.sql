USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiCreateQueuedJob]    Script Date: 11/11/2024 09:27:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER   PROCEDURE [dbo].[st_RobotApiCreateQueuedJob] 
(
	@REVIEW_ID int,
	@ROBOT_NAME nvarchar(255),
	@CRITERIA varchar(max),
	@CREDIT_PURCHASE_ID int,
	@REVIEW_SET_ID int,
	@FORCE_CODING_IN_ROBOT_NAME bit,
	@LOCK_CODING bit,
	@CONTACT_ID int,
	@RESULT varchar(100) output,
	@ROBOT_API_CALL_ID int output
)
AS
BEGIN
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	IF @rID is null OR @rID < 1
	BEGIN
		set @result = 'Robot not found';
		return;
	END
	declare @RobotContactId int = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)

	declare @chk int = (select count(*) from TB_REVIEW_CONTACT where REVIEW_ID = @REVIEW_ID and CONTACT_ID = @RobotContactId)
	if @chk < 1
	BEGIN
		declare @CR int 
		insert into TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID) values (@REVIEW_ID, @RobotContactId)
		SET @CR = SCOPE_IDENTITY()
		INSERT into TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME) values (@CR, 'Coding only')
	END

	INSERT into TB_ROBOT_API_CALL_LOG (CREDIT_PURCHASE_ID, REVIEW_ID, ROBOT_ID, REVIEW_SET_ID
			, CRITERIA, [STATUS], CURRENT_ITEM_ID, [DATE_CREATED] ,[DATE_UPDATED] ,[SUCCESS],[INPUT_TOKENS_COUNT]
			,[OUTPUT_TOKENS_COUNT] ,[COST] ,[FORCE_CODING_IN_ROBOT_NAME] ,[LOCK_CODING], CONTACT_ID)
		values
		(
			@CREDIT_PURCHASE_ID, @REVIEW_ID, @rID, @REVIEW_SET_ID
			, @CRITERIA, 'Queued', 0, GETDATE(), GETDATE(), 1, 0
			, 0, 0, @FORCE_CODING_IN_ROBOT_NAME, @LOCK_CODING, @CONTACT_ID
		);
	set @ROBOT_API_CALL_ID = SCOPE_IDENTITY();
	set @RESULT = 'Success';
END
GO
