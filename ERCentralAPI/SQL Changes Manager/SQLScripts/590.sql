Use Reviewer
GO

CREATE or ALTER PROCEDURE st_RobotApiJobFetchNextCreditTaksByRobotName 
(
	@ROBOT_NAME nvarchar(255)
)
AS
BEGIN
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	declare @RobotContactId int = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	select top 1 *, @RobotContactId as ROBOT_CONTACT_ID from TB_ROBOT_API_CALL_LOG 
		where ROBOT_ID = @rID and (STATUS = 'Queued' OR STATUS = 'Paused')
		order by DATE_CREATED asc
END
GO



IF COL_LENGTH('dbo.TB_ROBOT_API_CALL_LOG', 'CONTACT_ID') IS NULL
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

		ALTER TABLE TB_ROBOT_API_CALL_LOG
				ADD CONTACT_ID INT NULL
		
		ALTER TABLE TB_ROBOT_API_CALL_LOG ADD CONSTRAINT
			FK_TB_ROBOT__CONTACT_ID FOREIGN KEY	(CONTACT_ID)
				REFERENCES dbo.TB_CONTACT (CONTACT_ID) ON UPDATE NO ACTION ON DELETE CASCADE
	COMMIT
END
GO

CREATE or ALTER PROCEDURE st_RobotApiCreateQueuedJob 
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
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[st_CreateRobotApiCallLog]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[st_CreateRobotApiCallLog]
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiCallLogCreate]    Script Date: 21/05/2024 10:44:27 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[st_RobotApiCallLogCreate] 
(
	@REVIEW_ID int,
	@CREDIT_PURCHASE_ID int,
	@ROBOT_NAME nvarchar(255),
	@CRITERIA varchar(max),
	@REVIEW_SET_ID int,
	@CURRENT_ITEM_ID bigint,
	@FORCE_CODING_IN_ROBOT_NAME bit,
	@LOCK_CODING bit,
	@CONTACT_ID int,
	@result varchar(50) OUTPUT, 
	@JobId int OUTPUT,
	@RobotContactId int OUTPUT
)
AS
BEGIN

	set @result = 'Success'
	declare @rID int = (select ROBOT_ID from TB_CONTACT c 
							Inner join TB_ROBOT_ACCOUNT rc on c.CONTACT_ID = rc.CONTACT_ID and c.CONTACT_NAME = @ROBOT_NAME
							AND c.EMAIL like '%FAKE@ucl.ac.uk' AND c.EXPIRY_DATE is null);
	IF @rID is null OR @rID < 1
	BEGIN
		set @result = 'Robot not found';
		return;
	END
	set @RobotContactId = (select CONTACT_ID from TB_ROBOT_ACCOUNT where ROBOT_ID = @rID)
	declare @chk int = (Select count(*) from  tb_review_set where REVIEW_SET_ID = @REVIEW_SET_ID and REVIEW_ID = @REVIEW_ID)
	if @chk < 1
	BEGIN
		set @result = 'Coding tool not found';
		return;
	END
	set @chk = (select count(*) from TB_REVIEW_CONTACT where REVIEW_ID = @REVIEW_ID and CONTACT_ID = @RobotContactId)
	if @chk < 1
	BEGIN
		declare @CR int 
		insert into TB_REVIEW_CONTACT (REVIEW_ID, CONTACT_ID) values (@REVIEW_ID, @RobotContactId)
		SET @CR = SCOPE_IDENTITY()
		INSERT into TB_CONTACT_REVIEW_ROLE (REVIEW_CONTACT_ID, ROLE_NAME) values (@CR, 'Coding only')
	END

	insert into TB_ROBOT_API_CALL_LOG
		(CREDIT_PURCHASE_ID,REVIEW_ID,ROBOT_ID,REVIEW_SET_ID,CRITERIA,[STATUS], CURRENT_ITEM_ID 
		,SUCCESS,INPUT_TOKENS_COUNT,OUTPUT_TOKENS_COUNT,COST
		,FORCE_CODING_IN_ROBOT_NAME, LOCK_CODING, CONTACT_ID)
		VALUES
		(@CREDIT_PURCHASE_ID, @REVIEW_ID, @rID, @REVIEW_SET_ID, @CRITERIA, 'Starting', @CURRENT_ITEM_ID
		, 1, 0, 0, 0
		, @FORCE_CODING_IN_ROBOT_NAME, @LOCK_CODING, @CONTACT_ID);
	SET @JobId = SCOPE_IDENTITY();
END
GO