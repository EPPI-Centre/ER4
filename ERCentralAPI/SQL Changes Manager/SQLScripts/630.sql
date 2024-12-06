Use Reviewer
GO
IF COL_LENGTH('dbo.TB_ROBOT_API_CALL_LOG', 'USE_PDFS') IS NULL
BEGIN
	ALTER TABLE TB_ROBOT_API_CALL_LOG ADD USE_PDFS Bit NOT NULL
	CONSTRAINT D_TB_ROBOT_API_CALL_LOG_USE_PDFS
    DEFAULT (0)
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE or ALTER PROCEDURE st_GetItemDocumentIdsFromItemIds
	-- Add the parameters for the stored procedure here
	@ReviewId int,
	@ItemIds varchar(max),
	@AlsoFetchFromLinkedItems bit = 1

AS
BEGIN
	declare @t table (ItemId bigint primary key);
    insert into @t select value from dbo.fn_Split_int(@ItemIds, ',') s
		inner join TB_ITEM_REVIEW ir on s.value = ir.ITEM_ID and ir.REVIEW_ID = @ReviewId;

	IF @AlsoFetchFromLinkedItems = 1
	BEGIN
		insert into @t select ID1.ITEM_ID from @t t
			INNER JOIN TB_ITEM_LINK IL1 on ITEM_ID_PRIMARY = t.ItemId
			INNER JOIN TB_ITEM_DOCUMENT ID1 ON ID1.ITEM_ID = IL1.ITEM_ID_SECONDARY and IL1.ITEM_ID_PRIMARY != IL1.ITEM_ID_SECONDARY
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = ID1.ITEM_ID and IR.REVIEW_ID = @ReviewId
			where ID1.ITEM_ID not in (select ItemId from @t);
		insert into @t SELECT ID2.ITEM_ID FROM  @t t
			INNER JOIN TB_ITEM_LINK IL2 on IL2.ITEM_ID_SECONDARY = t.ItemId
			INNER JOIN TB_ITEM_DOCUMENT ID2 ON ID2.ITEM_ID = IL2.ITEM_ID_PRIMARY and IL2.ITEM_ID_PRIMARY != IL2.ITEM_ID_SECONDARY
			INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = ID2.ITEM_ID and IR.REVIEW_ID = @ReviewId
			WHERE ID2.ITEM_ID not in (select ItemId from @t);
	END
	select distinct id.ITEM_DOCUMENT_ID, ITEM_ID, id.DOCUMENT_EXTENSION from @t t 
		inner join TB_ITEM_DOCUMENT id on t.ItemId = id.ITEM_ID;
END

GO

CREATE OR ALTER PROCEDURE [dbo].[st_ParsePDFsCheckOngoingLog] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@CONTACT_ID int,
		@NewJobId int = 0 output
	)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT, XACT_ABORT ON;
	--Based on the template of st_ItemDuplicateGroupCheckOngoingLog
	--see https://weblogs.sqlteam.com/dang/2007/10/28/conditional-insertupdate-race-condition/
	--for details on how this is dealing with concurrency problems. Aim is to avoid having two instances of this SP insert
	--SP is called whenever the list of duplicates is retrieved AND when we are asking to find new duplicates...
	
	--paired with the "lasting lock", the transaction prevents two instances to be triggered concurrently
	--without the lasting lock, the TRAN itself won't work, see link above for the details.
	BEGIN TRAN A

	--get the last job ID, and acquire an exclusive table lock
	declare @jobid int = (select top 1 REVIEW_JOB_ID 
							from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK) 
							where REVIEW_ID = @revID AND JOB_TYPE = 'Parse PDFs' and SUCCESS is null
							order by REVIEW_JOB_ID desc)
	--TABLOCKX: exclusive table lock; HOLDLOCK: keep this lock on until the whole transaction is done.
	
	--how did the last job go?
	declare @state int = 0 --0 for not running, 1 for running normally, -1 for running and inactive for too long
	IF @jobid is not null set @state = dbo.fn_JobOfTypeIsRunning(@revID, 'Parse PDFs', 15);
	
	if @state = 0
		BEGIN
			--EITHER never done for this review, or last run has finished. 
			--we insert the "Parse PDFs" line here, to make this properly ACID
			--to be "Atomic" we also have the transaction statement AND the lock hints on the first lookup for the @jobid
			insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
				VALUES (@revID, @CONTACT_ID, getdate(),getdate(), 'Parse PDFs', 'Starting')
			set @NewJobId = SCOPE_IDENTITY()
				
			COMMIT TRAN A --Have to commit, before we can return!
			return 1
		END
	ELSE IF @state = -1
	BEGIN --has been "running" without updates for too long
		--we'll mark the job as failed and start a new one
		declare @NewMSG nvarchar(4000) = (select JOB_MESSAGE from tb_REVIEW_JOB where REVIEW_JOB_ID = @jobid);

		--making sure we never get a truncation error! LEFT(...) does work OK if we ask for more Chars than the string contains
		if @NewMSG is null set @NewMSG = 'Automated Failure: inactive for too long'
		else set @NewMSG = 'Automated Failure: inactive for too long' + CHAR(13)+ CHAR(10) + LEFT(@NewMSG,3950);
		 
		update tb_REVIEW_JOB set CURRENT_STATE = 'Failed', SUCCESS = 0, JOB_MESSAGE = @NewMSG where REVIEW_JOB_ID = @jobid

		insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
			VALUES (@revID, @CONTACT_ID, getdate(),getdate(), 'Parse PDFs', 'Starting')
		set @NewJobId = SCOPE_IDENTITY()
				
		COMMIT TRAN A --Have to commit, before we can return!
		return 1
	END
	ELSE --must be "1" for "running OK"
	BEGIN
		COMMIT TRAN A --Have to commit, before we can return!
		return -1
	END
	--We should never reach this, but just in case, we should return something...
	COMMIT TRAN A --Have to commit, before we can return!
	return -4; --signalling that something is WRONG!
	SET NOCOUNT OFF
END

GO

CREATE OR ALTER PROCEDURE [dbo].[st_LogReviewJobGetById]
	-- Add the parameters for the stored procedure here
	@ReviewId int,
	@JobId int
AS
BEGIN
	select * from tb_REVIEW_JOB where REVIEW_JOB_ID = @JobId and REVIEW_ID = @ReviewId;
END
GO
CREATE OR ALTER PROCEDURE [dbo].[st_LogReviewJobGetLatestMarkdownJob]
	-- Add the parameters for the stored procedure here
	@ReviewId int
AS
BEGIN
	select top 1 * from tb_REVIEW_JOB where REVIEW_ID = @ReviewId and JOB_TYPE = 'Parse PDFs'
	order by REVIEW_JOB_ID desc;
END
GO
USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiCreateQueuedJob]    Script Date: 18/11/2024 15:18:02 ******/
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
	@USE_PDFS bit,
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
			,[OUTPUT_TOKENS_COUNT] ,[COST] ,[FORCE_CODING_IN_ROBOT_NAME] ,[LOCK_CODING] , USE_PDFS, CONTACT_ID)
		values
		(
			@CREDIT_PURCHASE_ID, @REVIEW_ID, @rID, @REVIEW_SET_ID
			, @CRITERIA, 'Queued', 0, GETDATE(), GETDATE(), 1, 0
			, 0, 0, @FORCE_CODING_IN_ROBOT_NAME, @LOCK_CODING, @USE_PDFS, @CONTACT_ID
		);
	set @ROBOT_API_CALL_ID = SCOPE_IDENTITY();
	set @RESULT = 'Success';
END
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_RobotApiCallLogCreate]    Script Date: 18/11/2024 15:19:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_RobotApiCallLogCreate] 
(
	@REVIEW_ID int,
	@CREDIT_PURCHASE_ID int,
	@ROBOT_NAME nvarchar(255),
	@CRITERIA varchar(max),
	@REVIEW_SET_ID int,
	@CURRENT_ITEM_ID bigint,
	@FORCE_CODING_IN_ROBOT_NAME bit,
	@LOCK_CODING bit,
	@USE_PDFS bit,
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
		,FORCE_CODING_IN_ROBOT_NAME, LOCK_CODING, USE_PDFS, CONTACT_ID)
		VALUES
		(@CREDIT_PURCHASE_ID, @REVIEW_ID, @rID, @REVIEW_SET_ID, @CRITERIA, 'Starting', @CURRENT_ITEM_ID
		, 1, 0, 0, 0
		, @FORCE_CODING_IN_ROBOT_NAME, @LOCK_CODING, @USE_PDFS, @CONTACT_ID);
	SET @JobId = SCOPE_IDENTITY();
END
GO