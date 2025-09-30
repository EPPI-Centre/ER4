
USE [Reviewer]
GO

IF COL_LENGTH('dbo.TB_TRAINING_FROM_SEARCH', 'LOCAL_TP') IS NULL
BEGIN
	ALTER TABLE dbo.TB_TRAINING_FROM_SEARCH ADD
		LOCAL_TP int null, LOCAL_TN int null, LOCAL_TOT int null
END
GO


/****** Object:  StoredProcedure [dbo].[st_TrainingList]    Script Date: 06/08/2025 10:50:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER procedure [dbo].[st_TrainingFromSearchList]
(
	@REVIEW_ID INT
)

As

SET NOCOUNT ON
	
	SELECT *
	from TB_TRAINING_FROM_SEARCH t
	INNER JOIN TB_CONTACT ON TB_CONTACT.CONTACT_ID = t.CONTACT_ID
	WHERE REVIEW_ID = @REVIEW_ID
	order by t.TRAINING_FS_ID
	
SET NOCOUNT OFF
GO

CREATE OR ALTER procedure [dbo].[st_TrainingFromSearchRenewCounts]
(
	@REVIEW_ID int
	,@CONTACT_ID int
	,@CODE_SET_ID INT
	,@TRIGGERING_ITEM_ID bigint
	,@NEW_TRAINING_FS_ID int output
)
AS
SET NOCOUNT ON
	
	declare @LastFSid int = (Select max(TRAINING_FS_ID) from TB_TRAINING_FROM_SEARCH where REVIEW_ID = @REVIEW_ID);
	if @LastFSid is null OR @LastFSid < 1
	begin
		Set @NEW_TRAINING_FS_ID = -1;
		return;
	end
	
	DECLARE @TP INT;
	DECLARE @TN INT;
	DECLARE @LOCAL_TP INT;
	DECLARE @LOCAL_TN INT;
	DECLARE @ITEMS_IN_LIST int;
	--DECLARE @CODE_SET_ID INT = (SELECT SCREENING_CODE_SET_ID from TB_REVIEW where REVIEW_ID = @REVIEW_ID);
	DECLARE @SEARCH_ID INT;
	DECLARE @CurrentIteration int;
	SELECT @SEARCH_ID = SEARCH_ID, @CurrentIteration = ITERATION from TB_TRAINING_FROM_SEARCH where REVIEW_ID = @REVIEW_ID and TRAINING_FS_ID = @LastFSid;

	if @CODE_SET_ID is null OR @CODE_SET_ID < 1
		OR @SEARCH_ID is null OR @SEARCH_ID < 1
		OR @CurrentIteration is null OR @CurrentIteration < 1
	begin
		Set @NEW_TRAINING_FS_ID = -1;
		return;
	end

	if @TRIGGERING_ITEM_ID != 0 --auto-triggered, we may not need to do anything, so we'll check
	BEGIN
		--Check: is this item ALREADY coded by someone else? If it is, it has been "shown" to that other people already, so it almost certainly has already triggered this SP: we do NOT want
		DECLARE @codedCount int = (select count(ITEM_SET_ID) from tb_item_set where SET_ID = @CODE_SET_ID and ITEM_ID = @TRIGGERING_ITEM_ID and CONTACT_ID != @CONTACT_ID)
		
		-- ***** If this item is coded by someone else: RETURN
		if (@codedCount is not null AND @codedCount > 0) 
		begin
			Set @NEW_TRAINING_FS_ID = -2;
			RETURN;
		end
	END


	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	--again, but only for items in the current list!
	SELECT @LOCAL_TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_TRAINING_FROM_SEARCH_ITEM fsi
		INNER JOIN TB_ITEM_ATTRIBUTE on fsi.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID and fsi.TRAINING_FS_ID = @LastFSid
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @LOCAL_TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_TRAINING_FROM_SEARCH_ITEM fsi
		INNER JOIN TB_ITEM_ATTRIBUTE on fsi.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID and fsi.TRAINING_FS_ID = @LastFSid
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID
	
	SELECT @ITEMS_IN_LIST = LOCAL_TOT from TB_TRAINING_FROM_SEARCH where TRAINING_FS_ID = @LastFSid;

	INSERT Into TB_TRAINING_FROM_SEARCH ([CONTACT_ID], [REVIEW_ID], [DATE], [ITERATION]
       , [TRUE_POSITIVES], [TRUE_NEGATIVES], [SEARCH_ID]
	   , [LOCAL_TP], [LOCAL_TN], [LOCAL_TOT])
	 VALUES
		   (@CONTACT_ID, @REVIEW_ID, GETDATE(), @CurrentIteration + 1
		   , @TP, @TN,  @SEARCH_ID
		   , @LOCAL_TP, @LOCAL_TN, @ITEMS_IN_LIST)
	set @NEW_TRAINING_FS_ID = SCOPE_IDENTITY();

	--we could delete items that have been coded, but that's not a good idea: if, after this happened someone deletes or un-completes the coding of some item
	--that should be in this list, then the items involved will not get re-included in the list, and will thus not get coded/completed again

	--instead, we just update the reference to the latest TRAINING_FS_ID
	UPDATE TB_TRAINING_FROM_SEARCH_ITEM set TRAINING_FS_ID = @NEW_TRAINING_FS_ID where TRAINING_FS_ID = @LastFSid;


SET NOCOUNT OFF
GO

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_TrainingScreeningCheckOngoingLog]    Script Date: 06/08/2025 12:36:31 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER   PROCEDURE [dbo].[st_TrainingScreeningCheckOngoingLog] 
	-- Add the parameters for the stored procedure here
	(
		@revID int,
		@CONTACT_ID int,
		@TRIGGERING_ITEM_ID BIGINT = 0,
		@NewJobId int = 0 output,
		@NewTrainingId int = 0 output
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
	
	--added Aug 2025: check if the @TRIGGERING_ITEM_ID has been coded already, in which case we can return and do nothing: this item most likely has already triggered a training event.
	if @TRIGGERING_ITEM_ID != 0 --auto-triggered, we may not need to do anything, so we'll check
	BEGIN
		DECLARE @CODE_SET_ID int = (SELECT SCREENING_CODE_SET_ID from TB_REVIEW where REVIEW_ID = @revID);
		if @CODE_SET_ID is null OR @CODE_SET_ID < 1
		begin
			return -4;--signals that something is wrong
		end
		--Check: is this item ALREADY coded by someone else? If it is, it has been "shown" to that other people already, so it almost certainly has already triggered this SP: we do NOT want
		DECLARE @codedCount int = (select count(ITEM_SET_ID) from tb_item_set where SET_ID = @CODE_SET_ID and ITEM_ID = @TRIGGERING_ITEM_ID and CONTACT_ID != @CONTACT_ID)
		
		-- ***** If this item is coded by someone else: RETURN with the unique "didn't do it" result (-2)
		if (@codedCount is not null AND @codedCount > 0) 
		begin
			RETURN -2;
		end
	END

	--paired with the "lasting lock", the transaction prevents two instances to be triggered concurrently
	--without the lasting lock, the TRAN itself won't work, see link above for the details.
	BEGIN TRAN A

	--get the last job ID, and acquire an exclusive table lock
	declare @jobid int = (select top 1 REVIEW_JOB_ID 
							from tb_REVIEW_JOB WITH (TABLOCKX, HOLDLOCK) 
							where REVIEW_ID = @revID AND JOB_TYPE = 'PS training' and SUCCESS is null
							order by REVIEW_JOB_ID desc)
	--TABLOCKX: exclusive table lock; HOLDLOCK: keep this lock on until the whole transaction is done.
	
	--how did the last job go?
	declare @state int = 0 --0 for not running, 1 for running normally, -1 for running and inactive for too long
	IF @jobid is not null set @state = dbo.fn_JobOfTypeIsRunning(@revID, 'PS training', 15);
	
	if @state = 0
		BEGIN
			--EITHER never done for this review, or last run has finished. 
			--we insert the "PS training" line here, to make this properly ACID
			--to be "Atomic" we also have the transaction statement AND the lock hints on the first lookup for the @jobid
			insert into tb_REVIEW_JOB (REVIEW_ID, CONTACT_ID, START_TIME, END_TIME, JOB_TYPE, CURRENT_STATE) 
				VALUES (@revID, @CONTACT_ID, getdate(),getdate(), 'PS training', 'Starting')
			set @NewJobId = SCOPE_IDENTITY()

			insert into TB_TRAINING (REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
				VALUES (@revID, @CONTACT_ID, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
			set @NewTrainingId = SCOPE_IDENTITY()
				
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
			VALUES (@revID, @CONTACT_ID, getdate(),getdate(), 'PS training', 'Starting')
		set @NewJobId = SCOPE_IDENTITY()

		insert into TB_TRAINING (REVIEW_ID, CONTACT_ID, TIME_STARTED, TIME_ENDED)
			VALUES (@revID, @CONTACT_ID, getdate(),getdate())
		set @NewTrainingId = SCOPE_IDENTITY()
				
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

USE [Reviewer]
GO
/****** Object:  StoredProcedure [dbo].[st_ScreeningCreate_List_FromSearch]    Script Date: 07/08/2025 15:32:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER     procedure [dbo].[st_ScreeningCreate_List_FromSearch]
(
	@REVIEW_ID INT,
	@CONTACT_ID INT,
	@SEARCH_ID BIGINT,
	@CODE_SET_ID INT,
	@NEW_TRAINING_FS_ID int output
)

As

SET NOCOUNT ON
	-- ***** STEP 0 check that we have correct data
	declare @MaxSize int = (SELECT count(*) from TB_SEARCH s
								Inner join TB_SEARCH_ITEM si on s.SEARCH_ID = @SEARCH_ID and s.SEARCH_ID = si.SEARCH_ID and S.IS_CLASSIFIER_RESULT = 1
								AND s.REVIEW_ID = @REVIEW_ID);
	if (@MaxSize < 1 OR @MaxSize is null)
	BEGIN
		set @NEW_TRAINING_FS_ID = -1;
		return;
	END
	declare @IterationN int = 1 + (SELECT MAX(ITERATION) from TB_TRAINING_FROM_SEARCH where REVIEW_ID = @REVIEW_ID);
	if @IterationN is null set @IterationN = 1;

	DECLARE @TP INT
	DECLARE @TN INT
	DECLARE @LOCAL_TP INT;
	DECLARE @LOCAL_TN INT;
	DECLARE @ITEMS_IN_LIST int;
	DECLARE @ITEM_IDS table (ITEM_ID bigint primary key)

	Insert into @ITEM_IDS SELECT distinct SI.ITEM_ID from TB_SEARCH_ITEM SI
		INNER JOIN TB_ITEM_REVIEW IR ON IR.ITEM_ID = SI.ITEM_ID AND IR.REVIEW_ID = @REVIEW_ID and SI.SEARCH_ID = @SEARCH_ID
		WHERE NOT SI.ITEM_ID IN
			(SELECT ITEM_ID FROM TB_ITEM_SET WHERE IS_COMPLETED = 'TRUE' AND SET_ID = @CODE_SET_ID)
	SELECT @ITEMS_IN_LIST = @@ROWCOUNT;

	-- ***** FIRST, GET THE STATS IN TERMS OF # ITEMS SCREENED TO POPULATE THE TRIANING TABLE (GIVES US THE GRAPH ON THE SCREENING TAB)
	SELECT @TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM TB_ITEM_ATTRIBUTE
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID
	
	--again, but only for items in the search results!
	SELECT @LOCAL_TP = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM @ITEM_IDS t
		INNER JOIN TB_ITEM_ATTRIBUTE on t.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID 
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 10
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	SELECT @LOCAL_TN = COUNT(DISTINCT TB_ITEM_ATTRIBUTE.ITEM_ID) FROM  @ITEM_IDS t
		INNER JOIN TB_ITEM_ATTRIBUTE on t.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID 
		INNER JOIN TB_ATTRIBUTE_SET ON TB_ATTRIBUTE_SET.ATTRIBUTE_ID = TB_ITEM_ATTRIBUTE.ATTRIBUTE_ID AND TB_ATTRIBUTE_SET.ATTRIBUTE_TYPE_ID = 11
		INNER JOIN TB_ITEM_REVIEW ON TB_ITEM_REVIEW.ITEM_ID = TB_ITEM_ATTRIBUTE.ITEM_ID AND TB_ITEM_REVIEW.REVIEW_ID = @REVIEW_ID
		INNER JOIN TB_ITEM_SET ON TB_ITEM_SET.ITEM_SET_ID = TB_ITEM_ATTRIBUTE.ITEM_SET_ID
			AND TB_ITEM_SET.IS_COMPLETED = 'TRUE'
			AND TB_ITEM_SET.SET_ID = @CODE_SET_ID

	-- ********** SECOND, create new line in TB_TRAINING_FROM_SEARCH
	
	INSERT Into TB_TRAINING_FROM_SEARCH ([CONTACT_ID], [REVIEW_ID], [DATE], [ITERATION]
       , [TRUE_POSITIVES], [TRUE_NEGATIVES], [SEARCH_ID]
	   , [LOCAL_TP], [LOCAL_TN], [LOCAL_TOT])
	 VALUES
		   (@CONTACT_ID, @REVIEW_ID, GETDATE(), @IterationN
		   , @TP, @TN,  @SEARCH_ID
		   , @LOCAL_TP, @LOCAL_TN, @ITEMS_IN_LIST)
	set @NEW_TRAINING_FS_ID = SCOPE_IDENTITY();

	-- ********** THIRD, ENTER THE LIST OF ITEMS INTO TB_TRAINING_ITEM ACCORDING TO WHETHER WE'RE FILTERING BY AN ATTRIBUTE OR DOING THE WHOLE REVIEW
	declare @multiplier decimal(10,10);
	declare @MaxRank int = (Select max(ITEM_RANK) from TB_SEARCH_ITEM where SEARCH_ID = @SEARCH_ID)
	if (@MaxRank < 100) set @multiplier = 0.01;
	ELSE if (@MaxRank < 1000) set @multiplier = 0.001;
	ELSE if (@MaxRank < 10000) set @multiplier = 0.0001;
	ELSE if (@MaxRank < 100000) set @multiplier = 0.00001;
	ELSE if (@MaxRank < 1000000) set @multiplier = 0.000001;

	INSERT INTO TB_TRAINING_FROM_SEARCH_ITEM(TRAINING_FS_ID, ITEM_ID, CONTACT_ID_CODING, [RANK], SCORE)
		SELECT @NEW_TRAINING_FS_ID, SI.ITEM_ID, 0, 0, (CAST(SI.ITEM_RANK as decimal (20,10)) * @multiplier)
		FROM @ITEM_IDS t 
		INNER join TB_SEARCH_ITEM SI on t.ITEM_ID = SI.ITEM_ID and SI.SEARCH_ID = @SEARCH_ID
		ORDER BY SI.ITEM_RANK DESC
			
	

	/* SET THE RANKS TO INCREMENT */
	DECLARE @START_INDEX INT = 0
	SELECT @START_INDEX = MIN(TRAINING_FS_ITEM_ID) FROM TB_TRAINING_FROM_SEARCH_ITEM WHERE TRAINING_FS_ID = @NEW_TRAINING_FS_ID
	UPDATE TB_TRAINING_FROM_SEARCH_ITEM
		SET [RANK] = TRAINING_FS_ITEM_ID - @START_INDEX + 1
		WHERE TRAINING_FS_ID = @NEW_TRAINING_FS_ID


	-- FINALLY, MIGRATE ANY NON-STALE CODING LOCKS FROM THE PREVIOUS TRAINING RUN
	DECLARE @LAST_TRAINING_ID INT

	SELECT @LAST_TRAINING_ID = MAX(TRAINING_FS_ID) FROM TB_TRAINING_FROM_SEARCH
		WHERE REVIEW_ID = @REVIEW_ID AND TRAINING_FS_ID < (SELECT MAX(TRAINING_FS_ID) FROM TB_TRAINING_FROM_SEARCH WHERE REVIEW_ID = @REVIEW_ID)

	
	--get locks from previous list of FS type
	UPDATE A
		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
		A.WHEN_LOCKED = B.WHEN_LOCKED
		FROM TB_TRAINING_FROM_SEARCH_ITEM A
		JOIN
		TB_TRAINING_FROM_SEARCH_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(hour, 13, B.WHEN_LOCKED) AND
			B.TRAINING_FS_ID = @LAST_TRAINING_ID
		WHERE A.TRAINING_FS_ID = @NEW_TRAINING_FS_ID

	Declare @lastPStrainingId int = (select MAX(TRAINING_ID) FROM TB_TRAINING
		WHERE REVIEW_ID = @REVIEW_ID AND ITERATION is not null)
	--and again but for locks on the current list of PS type
	UPDATE A
		SET A.CONTACT_ID_CODING = B.CONTACT_ID_CODING,
		A.WHEN_LOCKED = B.WHEN_LOCKED
		FROM TB_TRAINING_FROM_SEARCH_ITEM A
		JOIN
		TB_TRAINING_ITEM B ON A.ITEM_ID = B.ITEM_ID AND CURRENT_TIMESTAMP < DATEADD(hour, 13, B.WHEN_LOCKED) AND
			B.TRAINING_ID = @lastPStrainingId
		WHERE A.TRAINING_FS_ID = @NEW_TRAINING_FS_ID
	

	-- delete the old list(s) of items to screen for this review
	DELETE TI
	FROM TB_TRAINING_FROM_SEARCH_ITEM TI
	INNER JOIN TB_TRAINING_FROM_SEARCH T ON T.TRAINING_FS_ID = TI.TRAINING_FS_ID
	WHERE T.REVIEW_ID = @REVIEW_ID AND T.TRAINING_FS_ID < @NEW_TRAINING_FS_ID

	

SET NOCOUNT OFF
GO